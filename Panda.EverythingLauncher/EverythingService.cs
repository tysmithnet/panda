using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    [Export]
    public class EverythingService
    {
        [Import]
        public SettingsService SettingsService { get; set; }

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
                    while (!process.HasExited)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            process.Kill();
                            observer.OnCompleted();
                            return;
                        }
                        
                        observer.OnNext(new EverythingResult
                        {
                            FullPath = await process.StandardOutput.ReadLineAsync()
                        });
                    }

                    process.WaitForExit(); // todo: make setting
                    observer.OnCompleted();
                }, cancellationToken);
            });
            return obs.Publish().RefCount();
        }
    }
}