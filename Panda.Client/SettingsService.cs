using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panda.Client
{
    [Export(typeof(SettingsService))]
    public class SettingsService
    {
        [ImportMany]
        protected internal IPluginSettings[] AllPluginSettings { get; set; }

        public IEnumerable<TSettings> Get<TSettings>() where TSettings : IPluginSettings
        {
            return AllPluginSettings.OfType<TSettings>();
        }
    }
}
