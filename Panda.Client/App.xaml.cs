﻿using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
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
        private readonly ILog Log = LogManager.GetLogger<App>();

        public LauncherSelector Selector { get; set; }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Log.Trace("Looking for MEF components");
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblyPaths = Directory.EnumerateFiles(assemblyPath, "Panda.*.dll")
                .Concat(Directory.EnumerateFiles(assemblyPath, "Panda.Client.exe"));
            var catalogs = assemblyPaths.Select(a => new AssemblyCatalog(a));
            var aggregateCatalog = new AggregateCatalog(catalogs);
            var compositionContainer = new CompositionContainer(aggregateCatalog);
            Selector = compositionContainer.GetExportedValue<LauncherSelector>();

            var settingService = compositionContainer.GetExportedValue<ISettingsService>();
            settingService.Setup(CancellationToken.None).Wait();
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