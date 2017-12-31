using System.Collections.Generic;
using System.ComponentModel.Composition;
using Panda.Client;

namespace Panda.AppLauncher
{
    [Export(typeof(IPluginSettings))]
    public class AppLauncherSettings : IPluginSettings
    {
        public IList<RegisteredApplication> RegisteredApplications { get; set; } = new List<RegisteredApplication>();
    }
}