using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Common.Logging;
using Newtonsoft.Json;

namespace Panda.Client
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        ///     System mutex to prevent multiple instances from being open at the same time
        /// </summary>
        private static readonly Semaphore SingleInstanceGuard;

        /// <summary>
        ///     Initializes the <see cref="App" /> class.
        /// </summary>
        static App()
        {
            SingleInstanceGuard = new Semaphore(0, 1, Assembly.GetExecutingAssembly().FullName, out var createdNew);

            if (!createdNew)
            {
                // activate the already created instance
                var current = Process.GetCurrentProcess();
                var other = Process.GetProcessesByName(current.ProcessName).FirstOrDefault(p => p.Id != current.Id);
                if (other != null)
                {
                    NativeMethods.SetForegroundWindow(other.MainWindowHandle);
                    NativeMethods.ShowWindow(other.MainWindowHandle, WindowShowStyle.Restore);
                }
                Environment.Exit(-2);
            }
        }

        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>
        ///     The log.
        /// </value>
        private ILog Log { get; } = LogManager.GetLogger<App>();

        /// <summary>
        ///     Gets or sets the selector.
        /// </summary>
        /// <value>
        ///     The selector.
        /// </value>
        private LauncherSelector Selector { get; set; }

        /// <summary>
        ///     Handles the OnStartup event of the App control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StartupEventArgs" /> instance containing the event data.</param>
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Log.Trace("Looking for MEF components");
            var bindings = GetBindings();
            var compositionContainer = CreateCompositionContainer(bindings);
            ManipulateCompositionContainer(compositionContainer);
            SetupSystemServices(compositionContainer);
            SetupComponentsRequiringSetup(compositionContainer);
            Selector.Show();
        }

        private Bindings GetBindings()
        {
            try
            {
                string json = File.ReadAllText("panda.bindings.json");
                var bindings = JsonConvert.DeserializeObject<Bindings>(json);
                return bindings;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to read bindings config file: {e.Message}");
                return new Bindings();
            }   
        }

        /// <summary>
        ///     Setups the components requiring setup.
        /// </summary>
        /// <param name="compositionContainer">The composition container.</param>
        private void SetupComponentsRequiringSetup(CompositionContainer compositionContainer)
        {
            var requiresSetup = compositionContainer.GetExportedValues<IRequiresSetup>();
            var requiresSetupCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var setupTasks = requiresSetup.Select(x => x.Setup(requiresSetupCts.Token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    Log.Error($"Setup task for {x.GetType()} was cancelled.");
                    return;
                }
                if (t.IsFaulted)
                    t.Exception?.Flatten().Handle(exception =>
                    {
                        Log.Error($"Setup task for {x.GetType()} failed: {exception.Message}");
                        return true;
                    });
            }, requiresSetupCts.Token)).ToArray();
            Task.WaitAll(setupTasks);
        }

        /// <summary>
        ///     Setups the system services.
        /// </summary>
        /// <param name="compositionContainer">The composition container.</param>
        private void SetupSystemServices(CompositionContainer compositionContainer)
        {
            var systemServices = compositionContainer.GetExportedValues<ISystemService>();
            var setupCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var systemServiceSetupTasks =
                systemServices.Select(service => service.Setup(setupCts.Token).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                        Log.Error($"Service failed to setup: {service.GetType().FullName}");

                    if (task.IsCanceled)
                        Log.Warn($"Service was cancelled during setup: {service.GetType().FullName}");
                }, setupCts.Token));

            try
            {
                Task.WaitAll(systemServiceSetupTasks.ToArray());
            }
            catch (Exception)
            {
                Log.Fatal($"One or more system  services failed during startup. Check the logs for more information.");
                Current.Shutdown(-1);
            }
        }

        /// <summary>
        ///     Manipulates the composition container.
        /// </summary>
        /// <param name="compositionContainer">The composition container.</param>
        private void ManipulateCompositionContainer(CompositionContainer compositionContainer)
        {
            compositionContainer.ComposeExportedValue<IScheduler>(
                new SynchronizationContextScheduler(SynchronizationContext.Current));
            

            var systemInfoService = new SystemInformationService();

            compositionContainer.ComposeExportedValue<ISystemInformationService>(systemInfoService);

            Selector = compositionContainer.GetExportedValue<LauncherSelector>();
        }

        /// <summary>
        ///     Creates the composition container.
        /// </summary>
        /// <param name="bindings"></param>
        /// <returns>CompositionContainer.</returns>
        private static CompositionContainer CreateCompositionContainer(Bindings bindings)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblyPaths = Directory.EnumerateFiles(assemblyPath, "Panda.*.dll").Select(x => Path.GetFileName(x)).ToList();
            assemblyPaths.Add("Panda.Client.exe");
            assemblyPaths = assemblyPaths.Except(bindings.BlackListedAssemblies).ToList();

            var assemblies = assemblyPaths.Select(s =>
            {
                try
                {
                    return Assembly.LoadFile(Path.GetFullPath(s));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to load {s} - {e.Message}");
                    return null;
                }
            }).Where(x => x != null);

            var types = assemblies.SelectMany(x => x.DefinedTypes).ToList();
            var blacklistedTypes = types.Where(x => bindings.BlackListedTypes.Contains(x.FullName)); // todo: really should be assembly qualified type
            var okTypes = types.Except(blacklistedTypes);
            var typeCatalog = new TypeCatalog(okTypes);                                                                             
            var aggregateCatalog = new AggregateCatalog(typeCatalog);
            var compositionContainer = new CompositionContainer(aggregateCatalog);
            return compositionContainer;
        }

        /// <summary>
        ///     Methods for manipulating the panda windows
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            internal static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            internal static extern bool ShowWindow(IntPtr hWnd,
                WindowShowStyle nCmdShow);
        }

        /// <summary>
        ///     Enumeration of the different ways of showing a window.
        /// </summary>
        internal enum WindowShowStyle : uint
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            Maximize = 3,
            ShowNormalNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActivate = 7,
            ShowNoActivate = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimized = 11
        }
    }
}