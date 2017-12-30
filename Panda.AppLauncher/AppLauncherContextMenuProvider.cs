using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Panda.Client;

namespace Panda.AppLauncher
{
    [Export(typeof(IRegisteredApplicationContextMenuProvider))]
    [Export(typeof(IFileSystemContextMenuProvider))]
    public class AppLauncherContextMenuProvider : IFileSystemContextMenuProvider, IRegisteredApplicationContextMenuProvider
    {
        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public AppLauncherRepository AppLauncherRepository { get; set; }

        public bool CanHandle(IEnumerable<FileInfo> fileInfos)
        {
            return fileInfos.Any();
        }

        public IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<FileInfo> fileInfos)
        {
            var appLauncherSettings = SettingsService.Get<AppLauncherSettings>().Single();
            var items = new List<MenuItem>();
            var menuItem = new MenuItem
            {
                Header = "Add to Applications"
            };

            foreach (var fileInfo in fileInfos)
            {
                menuItem.Click += (sender, args) =>
                {
                    var registerdApp = new RegisteredApplication
                    {
                        FullPath = fileInfo.FullName,
                        DisplayName = fileInfo.Name
                    };
                    appLauncherSettings.RegisteredApplications.Add(registerdApp);
                };
                items.Add(menuItem);
            }
            menuItem.Click += (sender, args) =>
            {
                SettingsService.Save(CancellationToken.None);
            };                                         
            return items;
        }

        public bool CanHandle(IEnumerable<RegisteredApplication> items)
        {
            return items.Any();
        }

        public IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<RegisteredApplication> items)
        {
            var menuItem = new MenuItem();
            menuItem.Header = "Remove from Applications";
            foreach (var registeredApplication in items)
            {
                menuItem.Click += (sender, args) => AppLauncherRepository.Remove(registeredApplication);
            }
            menuItem.Click += (sender, args) => AppLauncherRepository.Save();
            return new[] {menuItem};
        }
    }
}
