using System.Collections.Generic;

namespace Panda.Client
{
    public interface ILauncherService
    {
        IEnumerable<Launcher> Get();
        IEnumerable<Launcher> Search(string text);
    }
}