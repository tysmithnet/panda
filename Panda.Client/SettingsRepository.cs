using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panda.Client
{
    [Export(typeof(SettingsRepository))]
    public class SettingsRepository
    {
        [ImportMany]
        public IPluginSettings[] AllPluginSettings { get; set; }
    }

    public interface IPluginSettings
    {
        
    }
}
