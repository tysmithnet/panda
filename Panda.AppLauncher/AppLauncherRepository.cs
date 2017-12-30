using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Panda.Client;

namespace Panda.AppLauncher
{
    [Export(typeof(IRequiresSetup))]
    [Export(typeof(AppLauncherRepository))]
    public class AppLauncherRepository : IRequiresSetup
    {
        public IList<RegisteredApplication> RegisteredApplications { get; set; } = new List<RegisteredApplication>();

        [Import]
        public SettingsService SettingsService { get; set; }

        public IEnumerable<RegisteredApplication> Get()
        {
            return RegisteredApplications;
        }

        public Task Setup(CancellationToken cancellationToken)
        {
            var settings = SettingsService.Get<AppLauncherSettings>().Single();
            foreach (var registeredApplication in settings.RegisteredApplications)
            {
                var revivedApp = new RegisteredApplication
                {
                    DisplayName = registeredApplication.DisplayName,
                    FullPath = registeredApplication.FullPath
                };
                RegisteredApplications.Add(revivedApp);
            }
            return Task.CompletedTask;
        }
    }
}