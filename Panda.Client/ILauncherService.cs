using System.Collections.Generic;

namespace Panda.Client
{
    /// <summary>
    ///     Service that will manage the Launcher domain
    /// </summary>
    public interface ILauncherService
    {
        /// <summary>
        ///     Gets all current launchers
        /// </summary>
        /// <returns>All current launchers</returns>
        IEnumerable<Launcher> Get();

        /// <summary>
        ///     Searches for launchers given a regular expression
        /// </summary>
        /// <param name="regex">The text.</param>
        /// <returns>All current launchers</returns>
        IEnumerable<Launcher> Search(string regex);
    }
}