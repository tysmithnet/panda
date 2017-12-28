using System.ComponentModel.Composition;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    [Export(typeof(IPluginSettings))]
    public class EverythingSettings : IPluginSettings
    {
        public string EsExePath { get; set; } = @"C:\Program Files\Everything\es.exe";
    }
}