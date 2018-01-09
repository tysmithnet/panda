using System;
using System.Threading;

namespace Panda.Wikipedia
{
    /// <summary>
    ///     Service to interact with wikipedia api
    /// </summary>
    public interface IWikipediaService
    {
        /// <summary>
        ///     Get a small list of wikipedia potential matches
        /// </summary>
        /// <param name="search">The search.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        IObservable<WikipediaResult> AutoComplete(string search, CancellationToken cancellationToken);
    }
}