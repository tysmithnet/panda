using System;
using System.Threading;

namespace Panda.EverythingLauncher
{
    /// <summary>
    ///     Represents something capable of using everything to find files
    /// </summary>
    public interface IEverythingService
    {
        /// <summary>
        ///     Searches the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An observable stream of everything search results</returns>
        IObservable<EverythingResult> Search(string query, CancellationToken cancellationToken);
    }
}