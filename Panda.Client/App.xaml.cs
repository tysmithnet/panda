﻿using System;
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
using Common.Logging;

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
            var compositionContainer = CreateCompositionContainer();
            ManipulateCompositionContainer(compositionContainer);
            SetupSystemServices(compositionContainer);
            SetupComponentsRequiringSetup(compositionContainer);
            Selector.Show();
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

            Selector = compositionContainer.GetExportedValue<LauncherSelector>();
        }

        /// <summary>
        ///     Creates the composition container.
        /// </summary>
        /// <returns>CompositionContainer.</returns>
        private static CompositionContainer CreateCompositionContainer()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblyPaths = Directory.EnumerateFiles(assemblyPath, "Panda.*.dll")
                .Concat(Directory.EnumerateFiles(assemblyPath, "Panda.Client.exe"));
            var catalogs = assemblyPaths.Select(a => new AssemblyCatalog(a));
            var aggregateCatalog = new AggregateCatalog(catalogs);
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