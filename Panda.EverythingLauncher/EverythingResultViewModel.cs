using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    /// <summary>
    ///     View model for everything results
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public sealed class EverythingResultViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     The icon for this result
        /// </summary>
        private ImageSource _icon;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EverythingResultViewModel" /> class.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        public EverythingResultViewModel(string fullName)
        {
            FullName = fullName;
            Name = Path.GetFileName(fullName);
            Directory = Path.GetDirectoryName(fullName);
        }

        /// <summary>
        ///     Gets or sets the full name.
        /// </summary>
        /// <value>
        ///     The full name.
        /// </value>
        public string FullName { get; set; }

        /// <summary>
        ///     Gets or sets the directory.
        /// </summary>
        /// <value>
        ///     The directory.
        /// </value>
        public string Directory { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the icon.
        /// </summary>
        /// <value>
        ///     The icon.
        /// </value>
        public ImageSource Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Loads the icon.
        /// </summary>
        /// <returns></returns>
        public Task LoadIcon()
        {
            return Task.Run(() =>
            {
                try
                {
                    Icon = IconHelper.IconFromFilePath(FullName);
                }
                catch (Exception)
                {
                    // todo: fallback icon
                }
            });
        }

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}