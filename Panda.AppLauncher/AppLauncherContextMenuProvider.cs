using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Panda.Client;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     Context menu provider for the registered application domain
    /// </summary>
    /// <seealso cref="Panda.Client.IFileSystemContextMenuProvider" />
    /// <seealso cref="Panda.AppLauncher.IRegisteredApplicationContextMenuProvider" />
    [Export(typeof(IRegisteredApplicationContextMenuProvider))]
    [Export(typeof(IFileSystemContextMenuProvider))]
    public sealed class AppLauncherContextMenuProvider : IFileSystemContextMenuProvider,
        IRegisteredApplicationContextMenuProvider
    {
        /// <summary>
        ///     Gets or sets the registered application service.
        /// </summary>
        /// <value>
        ///     The registered application service.
        /// </value>
        [Import]
        internal IRegisteredApplicationService RegisteredApplicationService { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Determines whether this instance can handle the specified file infos.
        /// </summary>
        /// <param name="fileInfos">The file infos.</param>
        /// <returns>
        ///     <c>true</c> if this instance can handle the specified file infos; otherwise, <c>false</c>.
        /// </returns>
        public bool CanHandle(IEnumerable<FileInfo> fileInfos)
        {
            return fileInfos.Any();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the context menu items.
        /// </summary>
        /// <param name="fileInfos">The file infos.</param>
        /// <returns>The context menu items for the provided file infos</returns>
        public IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<FileInfo> fileInfos)
        {
            var menuItem = new MenuItem
            {
                Header = "Add to Applications"
            };

            foreach (var fileInfo in fileInfos)
                menuItem.Click += (sender, args) =>
                {
                    var registerdApp = new RegisteredApplication
                    {
                        FullPath = fileInfo.FullName,
                        DisplayName = fileInfo.Name
                    };
                    RegisteredApplicationService.Add(registerdApp);
                };
            menuItem.Click += (sender, args) => RegisteredApplicationService.Save();
            return new[] {menuItem};
        }

        /// <inheritdoc />
        /// <summary>
        ///     Determines whether this instance can handle the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns>
        ///     <c>true</c> if this instance can handle the specified items; otherwise, <c>false</c>.
        /// </returns>
        public bool CanHandle(IEnumerable<RegisteredApplication> items)
        {
            return items.Any();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the context menu items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns>The context menu items for the provided registered applications</returns>
        public IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<RegisteredApplication> items)
        {
            var menuItem = new MenuItem {Header = "Remove from Applications"};
            foreach (var registeredApplication in items)
                menuItem.Click += (sender, args) => RegisteredApplicationService.Remove(registeredApplication);
            menuItem.Click += (sender, args) => RegisteredApplicationService.Save();
            return new[] {menuItem};
        }
    }
}