using System.Collections.Generic;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     Represents something that has been registered with the application launcher
    /// </summary>
    public sealed class LaunchableApplication
    {
        /// <summary>
        ///     Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get; set; }

        /// <summary>
        ///     Gets or sets the full path to the file
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath { get; set; }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the icon path. If this is null then the shell will be used to retrieve the icon
        ///     you see in Explorer.
        /// </summary>
        /// <value>The icon path.</value>
        public string IconPath { get; set; }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is LaunchableApplication application &&
                   DisplayName == application.DisplayName &&
                   FullPath == application.FullPath &&
                   Description == application.Description &&
                   IconPath == application.IconPath;
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            var hashCode = -1507886759;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DisplayName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullPath);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(IconPath);
            return hashCode;
        }
    }
}