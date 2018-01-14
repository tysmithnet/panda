using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Panda.CommonControls.Annotations;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    ///     Class ClipboardItemViewModel.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="ClipboardItemViewModel" />
    internal class ClipboardItemViewModel : INotifyPropertyChanged, IEquatable<ClipboardItemViewModel>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClipboardItemViewModel" /> class.
        /// </summary>
        /// <param name="textClipboardItem">The text clipboard item.</param>
        public ClipboardItemViewModel(TextClipboardItem textClipboardItem)
        {
            Instance = textClipboardItem;
            Preview = new string(textClipboardItem.Content.Take(250).ToArray());
        }

        /// <summary>
        ///     Gets or sets the preview.
        /// </summary>
        /// <value>The preview.</value>
        public string Preview { get; set; }

        /// <summary>
        ///     Gets or sets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public ClipboardItem Instance { get; set; }

        /// <summary>
        ///     Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns><c>true</c> if this instance is logically equal to other, <c>false</c> otherwise.</returns>
        public bool Equals(ClipboardItemViewModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Instance, other.Instance);
        }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            return Equals((ClipboardItemViewModel) obj);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return Instance != null ? Instance.GetHashCode() : 0;
        }

        /// <summary>
        ///     Implements the == operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(ClipboardItemViewModel left, ClipboardItemViewModel right)
        {
            return Equals(left, right);
        }

        /// <summary>
        ///     Implements the != operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(ClipboardItemViewModel left, ClipboardItemViewModel right)
        {
            return !Equals(left, right);
        }
    }
}