using System;
using System.Windows;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    ///     Class TextClipboardItem.
    /// </summary>
    /// <seealso cref="Panda.ClipboardLauncher.ClipboardItem" />
    /// <seealso cref="TextClipboardItem" />
    internal class TextClipboardItem : ClipboardItem, IEquatable<TextClipboardItem>
    {
        /// <summary>
        ///     Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        public string Content { get; set; }

        /// <summary>
        ///     Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns><c>true</c> if this instance is logically equal to other, <c>false</c> otherwise.</returns>
        public bool Equals(TextClipboardItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Content, other.Content);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TextClipboardItem) obj);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return Content != null ? Content.GetHashCode() : 0;
        }

        /// <summary>
        ///     Implements the == operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(TextClipboardItem left, TextClipboardItem right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Implements the != operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(TextClipboardItem left, TextClipboardItem right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        ///     Sets the clipboard.
        /// </summary>
        public override void SetClipboard()
        {
            Clipboard.SetText(Content);
        }
    }
}