using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Panda.Client;

namespace Panda.AppLauncher
{
    [Export(typeof(IFileSystemContextMenuProvider))]
    public class AppLauncherContextMenuProvider : IFileSystemContextMenuProvider
    {
        public bool CanHandle(IEnumerable<FileInfo> fileInfos)
        {
            return true;
        }

        public IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<FileInfo> fileInfos)
        {
            var menuItem = new MenuItem
            {
                Header = "Add to App Launcher",
            };
            menuItem.Click += (sender, args) =>
            {
                Console.WriteLine("got one");
            };
            return new[] {menuItem};
        }
    }
}
