using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Panda.Client
{
    [Export(typeof(ILauncherService))]
    public sealed class LauncherService : ILauncherService
    {
        [ImportMany]
        internal Launcher[] Launchers { get; set; }
        
        public IEnumerable<Launcher> Search(string text)
        {
            return Launchers.Where(launcher => launcher.GetType().FullName.ToLower().Contains(text));
        }

        public IEnumerable<Launcher> Get()
        {
            return Launchers;
        }
    }
}