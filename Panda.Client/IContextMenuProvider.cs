using System.Collections.Generic;
using System.Windows;

namespace Panda.Client
{
    public interface IContextMenuProvider<in TItem>
    {
        bool CanHandle(IEnumerable<TItem> items);
        IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<TItem> items);
    }
}