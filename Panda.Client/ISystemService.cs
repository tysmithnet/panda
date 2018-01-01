using System.Threading;
using System.Threading.Tasks;

namespace Panda.Client
{
    /// <summary>
    ///     Represents a service so fundamental that it requires its own lifetime management.
    ///     These services are meant to be readily consumed by plugin services.
    /// </summary>
    public interface ISystemService
    {
        /// <summary>
        ///     Performs any required setup
        /// </summary>
        /// <param name="token">The cancellation token</param>
        /// <returns>A task, that when complete, will indicate the completion of the setup process</returns>
        Task Setup(CancellationToken token);
    }
}