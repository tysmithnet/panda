using System;
using System.Threading;

namespace Panda.EverythingLauncher
{
    /// <summary>
    ///     Represents something capable of using everything to find files
    /// </summary>
    public interface IEverythingService // todo: should be more generic of a file system searcher
    {
        /// <summary>
        ///     Searches the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        IObservable<EverythingResult> Search(string query, CancellationToken cancellationToken);
    }
}