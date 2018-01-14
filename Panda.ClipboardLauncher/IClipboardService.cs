using System;
using System.Collections.Generic;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    ///     Interface IClipboardService
    /// </summary>
    internal interface IClipboardService
    {
        /// <summary>
        ///     Gets this instance.
        /// </summary>
        /// <typeparam name="TItem">The type of the t item.</typeparam>
        /// <returns>IObservable&lt;TItem&gt;.</returns>
        IObservable<TItem> Get<TItem>() where TItem : ClipboardItem;

        /// <summary>
        ///     Searches the specified s.
        /// </summary>
        /// <typeparam name="TItem">The type of the t item.</typeparam>
        /// <param name="s">The s.</param>
        /// <returns>IEnumerable&lt;TItem&gt;.</returns>
        IEnumerable<TItem> Search<TItem>(string s) where TItem : ClipboardItem;

        /// <summary>
        ///     Sets the clipboard.
        /// </summary>
        /// <param name="item">The item.</param>
        void SetClipboard(ClipboardItem item);
    }
}