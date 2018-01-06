using System;
using System.IO;
using System.Threading;

namespace Panda.Client
{
    /// <summary>
    ///     Represents something that is capable of searching the file system
    /// </summary>
    public interface IFileSystemSearch
    {
        /// <summary>
        ///     Searches for files using the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        IObservable<FileInfo> Search(string query, CancellationToken cancellationToken);
    }
}