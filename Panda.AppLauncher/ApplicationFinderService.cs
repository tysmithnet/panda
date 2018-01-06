using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Panda.Client;

namespace Panda.AppLauncher
{
    [Export(typeof(IRequiresSetup))]
    [Export(typeof(ApplicationFinderService))]
    public sealed class ApplicationFinderService : IRequiresSetup
    {
        [Import]
        internal IFileSystemSearch FileSystemSearch { get; set; }

        [Import]
        internal ISettingsService SettingsService { get; set; }

        internal AppLauncherSettings AppLauncherSettings { get; set; }

        public Task Setup(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
