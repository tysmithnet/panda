using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    [Export]
    public class EverythingService
    {
        private ILog Log { get; } = LogManager.GetLogger<EverythingService>();

        [Import]
        protected internal SettingsService SettingsService { get; set; }

        public IObservable<EverythingResult> Search(string query, CancellationToken cancellationToken)
        {
            var executablePath = SettingsService.Get<EverythingSettings>().Single().EsExePath;
            var obs = Observable.Create<EverythingResult>(async (observer, token) =>
            {
                await Task.Run(async () =>
                {
                    var process = new Process
                    {
                        StartInfo =
                        {
                            FileName = executablePath,
                            Arguments = query,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    Log.Debug($"Started: {process.Id}");
                    string line;
                    while ((line = await process.StandardOutput.ReadLineAsync()) != null)
                    {
                        Log.Debug($"Process Line: {process.Id} - {line}");
                        observer.OnNext(new EverythingResult
                        {
                            FullPath = line
                        });

                        if (!cancellationToken.IsCancellationRequested && !token.IsCancellationRequested) continue;
                        Log.Debug($"Killing: {process.Id}");
                        process.Kill();
                        observer.OnCompleted();
                        return;
                    }
                    Log.Debug($"Finished: {process.Id}");
                    process.Kill();
                    observer.OnCompleted();
                }, cancellationToken);
            });
            return obs.Publish().RefCount();
        }
    }
}