using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data;
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
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected ILog Log = LogManager.GetLogger<App>();

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Log.Trace("Looking for MEF components");
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblyPaths = Directory.EnumerateFiles(assemblyPath, "Panda.*.dll")
                .Concat(Directory.EnumerateFiles(assemblyPath, "Panda.Client.exe"));
            var catalogs = assemblyPaths.Select(a => new AssemblyCatalog(a));
            var aggregateCatalog = new AggregateCatalog(catalogs);
            var compositionContainer = new CompositionContainer(aggregateCatalog);  
            var selector = compositionContainer.GetExportedValue<LauncherSelector>();

            var settingService = compositionContainer.GetExportedValue<SettingsService>();
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
                {
                    t.Exception?.Flatten().Handle(exception =>
                    {
                        Log.Error($"Setup task for {x.GetType()} failed: {exception.Message}");
                        return true;
                    });
                }
            })).ToArray();
            Task.WaitAll(setupTasks);
            selector.Show();
        }
    }
}
