using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reflection;
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
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblyPaths = Directory.EnumerateFiles(assemblyPath, "Panda.*.dll")
                .Concat(Directory.EnumerateFiles(assemblyPath, "Panda.Client.exe"));
            var catalogs = assemblyPaths.Select(a => new AssemblyCatalog(a));
            var aggregateCatalog = new AggregateCatalog(catalogs);
            var compositionContainer = new CompositionContainer(aggregateCatalog);
            compositionContainer.ComposeExportedValue<IScheduler>(Scheduler.CurrentThread);
            Selector = compositionContainer.GetExportedValue<LauncherSelector>();

            var systemServices = compositionContainer.GetExportedValues<ISystemService>();
            var systemServiceSetupTasks =
                systemServices.Select(service => service.Setup(CancellationToken.None).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                        Log.Error($"Service failed to setup: {service.GetType().FullName}");

                    if (task.IsCanceled)
                        Log.Warn($"Service was cancelled during setup: {service.GetType().FullName}");
                })); // todo: use real CT

            try
            {
                Task.WaitAll(systemServiceSetupTasks.ToArray());
            }
            catch (Exception)
            {
                Log.Fatal($"One or more system  services failed during startup. Check the logs for more information.");
                Current.Shutdown(-1);
            }
            // todo: break up method
            var requiresSetup = compositionContainer.GetExportedValues<IRequiresSetup>();
            var setupTasks = requiresSetup.Select(x => x.Setup(CancellationToken.None).ContinueWith(t =>
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
            })).ToArray();
            Task.WaitAll(setupTasks);
            Selector.Show();
        }
    }                                       
}