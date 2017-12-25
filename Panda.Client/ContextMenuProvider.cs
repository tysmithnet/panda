using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Panda.Client
{
    public interface IFileSystemContextMenuProvider
    {
        bool CanHandle(IEnumerable<FileInfo> fileInfos);
        IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<FileInfo> fileInfos);
    }
}
