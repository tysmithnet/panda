using System.Threading;
using System.Threading.Tasks;

namespace Panda.Client
{
    public interface ISystemService
    {
        Task Setup(CancellationToken token);
    }
}