using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Panda.Client;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     Context menu provider for the registered application domain
    /// </summary>
    /// <seealso cref="Panda.AppLauncher.ILaunchableApplicationContextMenuProvider" />
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
        /// <value>The registered application service.</value>
        [Import]
        private ILaunchableApplicationService LaunchableApplicationService { get; set; }

        /// <summary>
        ///     Determines whether this instance can handle the specified file infos.
        /// </summary>
        /// <param name="fileInfos">The file infos.</param>
        /// <returns>
        ///     <c>true</c> if this instance can handle the specified file infos; otherwise, <c>false</c>.
        /// </returns>
        /// <inheritdoc />
        public bool CanHandle(IEnumerable<FileInfo> fileInfos)
        {
            return fileInfos.Any();
        }

        /// <summary>
        ///     Gets the context menu items.
        /// </summary>
        /// <param name="fileInfos">The file infos.</param>
        /// <returns>The context menu items for the provided file infos</returns>
        /// <inheritdoc />
        public IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<FileInfo> fileInfos)
        {
            var addToApplicationsMenuItem = CreateAddToApplicationsMenuItem(fileInfos);
            return new[] {addToApplicationsMenuItem};
        }

        /// <summary>
        ///     Determines whether this instance can handle the specified items.
        /// </summary>
        /// <param name="launchableApplicationViewModels">The items.</param>
        /// <returns>
        ///     <c>true</c> if this instance can handle the specified items; otherwise, <c>false</c>.
        /// </returns>
        /// <inheritdoc />
        public bool CanHandle(IEnumerable<LaunchableApplicationViewModel> launchableApplicationViewModels)
        {
            return launchableApplicationViewModels.Any();
        }

        /// <summary>
        ///     Gets the context menu items.
        /// </summary>
        /// <param name="launchableApplicationViewModels">The items.</param>
        /// <returns>The context menu items for the provided registered applications</returns>
        /// <inheritdoc />
        public IEnumerable<FrameworkElement> GetContextMenuItems(
            IEnumerable<LaunchableApplicationViewModel> launchableApplicationViewModels)
        {
            launchableApplicationViewModels = launchableApplicationViewModels.ToList();

            var removeMenuItem = CreateRemoveMenuItem(launchableApplicationViewModels);
            var editMenuItem = CreateEditMenuItem(launchableApplicationViewModels);    
            var changeIconMenuItem = CreateChangeIconMenuItem(launchableApplicationViewModels);

            return new[] {removeMenuItem, editMenuItem, changeIconMenuItem};
        }

        /// <summary>
        ///     Creates the change icon menu item.
        /// </summary>
        /// <param name="vms">The VMS.</param>
        /// <returns>System.Windows.Controls.MenuItem.</returns>
        private MenuItem CreateChangeIconMenuItem(IEnumerable<LaunchableApplicationViewModel> vms)
        {
            var menuItem = new MenuItem
            {
                Header = "Change Icon"
            };

            menuItem.Click += (sender, args) =>
            {
                var dlg = new OpenFileDialog
                {
                    DefaultExt = ".exe",
                    Filter = "Image Files |*.ico|*.png|*.jpg|*.jpeg|*.gif|All Files (*.*)|*.*"
                };

                var result = dlg.ShowDialog();

                if (result != true) return;
                var filename = dlg.FileName;
                foreach (var launchableApplicationViewModel in vms)
                {
                    launchableApplicationViewModel.Instance.IconPath = filename;
                    launchableApplicationViewModel.LoadIcon(IconSize.Large);
                }

                LaunchableApplicationService.Save();
            };

            return menuItem;
        }

        /// <summary>
        ///     Creates the add to applications menu item.
        /// </summary>
        /// <param name="fileInfos">The file infos.</param>
        /// <returns>System.Windows.Controls.MenuItem.</returns>
        private MenuItem CreateAddToApplicationsMenuItem(IEnumerable<FileInfo> fileInfos)
        {
            var menuItem = new MenuItem
            {
                Header = "Add to Applications"
            };

            foreach (var fileInfo in fileInfos)
                menuItem.Click += (sender, args) =>
                {
                    var shellInfo = ShellHelper.GetShellFileInfo(fileInfo.FullName);
                    var registerdApp = new LaunchableApplication
                    {
                        FullPath = fileInfo.FullName,
                        DisplayName = shellInfo.DisplayName,
                        Description = shellInfo.Description
                    };
                    LaunchableApplicationService.Add(registerdApp);
                };
            menuItem.Click += (sender, args) => LaunchableApplicationService.Save();
            return menuItem;
        }

        /// <summary>
        ///     Creates the edit menu item.
        /// </summary>
        /// <param name="launchableApplicationViewModels">The launchable application view models.</param>
        /// <returns>System.Windows.Controls.MenuItem.</returns>
        private static MenuItem CreateEditMenuItem(
            IEnumerable<LaunchableApplicationViewModel> launchableApplicationViewModels)
        {
            var editMenuItem = new MenuItem {Header = "Edit"};
            foreach (var launchableApplicationViewModel in launchableApplicationViewModels)
                editMenuItem.Click += (sender, args) => { launchableApplicationViewModel.IsEditable = true; };
            return editMenuItem;
        }

        /// <summary>
        ///     Creates the remove menu item.
        /// </summary>
        /// <param name="launchableApplicationViewModels">The launchable application view models.</param>
        /// <returns>System.Windows.Controls.MenuItem.</returns>
        private MenuItem CreateRemoveMenuItem(
            IEnumerable<LaunchableApplicationViewModel> launchableApplicationViewModels)
        {
            var removeMenuItem = new MenuItem {Header = "Remove from Applications"};
            foreach (var launchableApplication in launchableApplicationViewModels)
                removeMenuItem.Click += (sender, args) =>
                    LaunchableApplicationService.Remove(launchableApplication.Instance);
            removeMenuItem.Click += (sender, args) => LaunchableApplicationService.Save();
            return removeMenuItem;
        }
    }
}