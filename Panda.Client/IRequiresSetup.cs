using System.Threading;
using System.Threading.Tasks;

namespace Panda.Client
{
    /// <summary>
    ///     Tagging interface for any class that requires some initial setup. At this point
    ///     system services are available for use.
    /// </summary>
    public interface IRequiresSetup
    {
        /// <summary>
        ///     Performs any setup logic required
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task, that when complete, will indicate that the setup is complete</returns>
        Task Setup(CancellationToken cancellationToken);
    }
}