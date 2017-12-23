using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Panda.Client;

namespace Panda.AppLauncher
{
    [Export(typeof(IPluginSettings))]
    public class AppLauncherSettings : IPluginSettings
    {                                    
    }
}
