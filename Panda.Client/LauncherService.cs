using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Panda.Client
{
    /// <inheritdoc />
    /// <summary>
    ///     Service that will manage the launcher domain
    /// </summary>
    /// <seealso cref="T:Panda.Client.ILauncherService" />
    [Export(typeof(ILauncherService))]
    internal sealed class LauncherService : ILauncherService
    {
        /// <summary>
        ///     Gets or sets the launchers.
        /// </summary>
        /// <value>
        ///     The launchers.
        /// </value>
        [ImportMany]
        private Launcher[] Launchers { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Searches for launchers given a regular expression
        /// </summary>
        /// <param name="regex">The text.</param>
        /// <returns>
        ///     All current launchers
        /// </returns>
        public IEnumerable<Launcher> Search(string regex)
        {
            return Launchers.Where(launcher => launcher.GetType().FullName.ToLower().Contains(regex));
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets all current launchers
        /// </summary>
        /// <returns>
        ///     All current launchers
        /// </returns>
        public IEnumerable<Launcher> Get()
        {
            return Launchers;
        }
    }
}