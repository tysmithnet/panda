using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Panda.Client;

namespace Panda.AppLauncher
{
    [Export(typeof(IRegisteredApplicationContextMenuProvider))]
    [Export(typeof(IFileSystemContextMenuProvider))]
    public sealed class AppLauncherContextMenuProvider : IFileSystemContextMenuProvider,
        IRegisteredApplicationContextMenuProvider
    {
        [Import]
        internal IRegisteredApplicationService RegisteredApplicationService { get; set; }

        public bool CanHandle(IEnumerable<FileInfo> fileInfos)
        {
            return fileInfos.Any();
        }

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

        public bool CanHandle(IEnumerable<RegisteredApplication> items)
        {
            return items.Any();
        }

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