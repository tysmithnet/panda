using System;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    ///     Class ClipboardItem.
    /// </summary>
    internal abstract class ClipboardItem
    {
        /// <summary>
        ///     Gets or sets the clipped date.
        /// </summary>
        /// <value>The clipped date.</value>
        public DateTime ClippedDate { get; set; }

        /// <summary>
        ///     Sets the clipboard.
        /// </summary>
        public abstract void SetClipboard();
    }
}