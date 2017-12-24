using System.Threading;
using System.Threading.Tasks;

namespace Panda.Client
{
    public interface IRequiresSetup
    {
        Task Setup(CancellationToken cancellationToken);
    }
}