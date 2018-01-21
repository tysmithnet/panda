using System.Collections.Generic;
using System.Windows;

namespace Panda.Client
{
    /// <summary>
    ///     Represents an object that is capable of creating context menu items
    /// </summary>
    /// <typeparam name="TItem">The type of selected item</typeparam>
    public interface IContextMenuProvider<in TItem>
    {
        /// <summary>
        ///     Determines whether this instance can handle the specified items.
        /// </summary>
        /// <param name="launchableApplicationViewModels">The items.</param>
        /// <returns>
        ///     <c>true</c> if this instance can handle the specified items; otherwise, <c>false</c>.
        /// </returns>
        bool CanHandle(IEnumerable<TItem> launchableApplicationViewModels);

        /// <summary>
        ///     Gets the context menu items for the provided items.
        /// </summary>
        /// <param name="launchableApplicationViewModels">The items.</param>
        /// <returns>The items to go in the ContextMenu</returns>
        IEnumerable<FrameworkElement> GetContextMenuItems(IEnumerable<TItem> launchableApplicationViewModels);
    }
}