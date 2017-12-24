using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    public class EverythingService
    {
        [Import]
        public SettingsService SettingsService { get; set; }

        public IObservable<EverythingResult> Search(string query)
        {
            var executablePath = SettingsService.Get<EverythingSettings>().Single().EsExePath;
            var obs = Observable.Create<EverythingResult>(async (observer, token) =>
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
                    observer.OnNext(new EverythingResult
                    {
                        FullPath = await process.StandardOutput.ReadLineAsync()
                    });
                process.WaitForExit(1000);  // todo: make setting
                observer.OnCompleted();
            });
            return obs.Publish().RefCount();
        }
    }
}
