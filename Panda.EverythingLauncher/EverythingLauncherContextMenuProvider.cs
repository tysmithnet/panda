using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Common.Logging;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    /// <inheritdoc />
    /// <summary>
    ///     Context menu provider for everything launcher
    /// </summary>
    /// <seealso cref="T:Panda.Client.IFileSystemContextMenuProvider" />
    [Export(typeof(IFileSystemContextMenuProvider))]
    public sealed class EverythingLauncherContextMenuProvider : IFileSystemContextMenuProvider
    {
        /// <summary>
        ///     Gets or sets the event hub.
        /// </summary>
        /// <value>
        ///     The event hub.
        /// </value>
        [Import]
        internal IEventHub EventHub { get; set; }

        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>
        ///     The log.
        /// </value>
        private ILog Log { get; } = LogManager.GetLogger<EverythingLauncherContextMenuProvider>();

        /// <inheritdoc />
        /// <summary>
        ///     Determines whether this instance can handle the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns>
        ///     <c>true</c> if this instance can handle the specified items; otherwise, <c>false</c>.
        /// </returns>
        public bool CanHandle(IEnumerable<FileInfo> items)
        {
            return false;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the context menu items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<FileInfo> items)
        {
            var menuItem = new MenuItem();
            var arr = items.ToArray();
            menuItem.Header = $"Delete {arr.Length} Items";

            menuItem.Click += (sender, args) =>
            {
                foreach (var fileInfo in arr)
                    try
                    {
                        File.Delete(fileInfo.FullName);
                        EventHub.Broadcast(new FileDeletedEvent(fileInfo.FullName));
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Could not delete {fileInfo.FullName} - {e.Message}");
                    }
            };

            return new[] {menuItem};
        }
    }
}