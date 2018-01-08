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
    /// <seealso cref="ILaunchableApplicationContextMenuProvider" />
    [Export(typeof(ILaunchableApplicationContextMenuProvider))]
    [Export(typeof(IFileSystemContextMenuProvider))]
    public sealed class LaunchableApplicationContextMenuProvider : IFileSystemContextMenuProvider,
        ILaunchableApplicationContextMenuProvider
    {
        /// <summary>
        ///     Gets or sets the registered application service.
        /// </summary>
        /// <value>
        ///     The registered application service.
        /// </value>
        [Import]
        internal ILaunchableApplicationService LaunchableApplicationService { get; set; }

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
                    var registerdApp = new LaunchableApplication
                    {
                        FullPath = fileInfo.FullName,
                        DisplayName = fileInfo.Name
                    };
                    LaunchableApplicationService.Add(registerdApp);
                };
            menuItem.Click += (sender, args) => LaunchableApplicationService.Save();
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
        public bool CanHandle(IEnumerable<LaunchableApplicationViewModel> items)
        {
            return items.Any();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the context menu items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns>The context menu items for the provided registered applications</returns>
        public IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<LaunchableApplicationViewModel> items)
        {
            items = items.ToList();
            var removeMenuItem = new MenuItem {Header = "Remove from Applications"};
            foreach (var launchableApplication in items)
                removeMenuItem.Click += (sender, args) =>
                    LaunchableApplicationService.Remove(launchableApplication.LaunchableApplication);
            removeMenuItem.Click += (sender, args) => LaunchableApplicationService.Save();

            var editMenuItem = new MenuItem {Header = "Edit"};
            foreach (var launchableApplicationViewModel in items)
                editMenuItem.Click += (sender, args) => { launchableApplicationViewModel.IsEditable = true; };

            return new[] {removeMenuItem, editMenuItem};
        }
    }
}