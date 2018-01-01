using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Panda.Client
{
    public interface ISettingsService
    {
        IEnumerable<TSettings> Get<TSettings>() where TSettings : IPluginSettings;
        void Save();
        Task Setup(CancellationToken cancellationToken);
    }
}