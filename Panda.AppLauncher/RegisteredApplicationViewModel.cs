using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Panda.Client;
using Panda.Client.Properties;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     View model for registered applications
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public sealed class RegisteredApplicationViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     The image source for the icon
        /// </summary>
        private ImageSource _imageSource;

        /// <summary>
        ///     Gets or sets the name of the application.
        /// </summary>
        /// <value>
        ///     The name of the application.
        /// </value>
        public string AppName { get; set; }

        /// <summary>
        ///     Gets or sets the executable location.
        /// </summary>
        /// <value>
        ///     The executable location.
        /// </value>
        public string ExecutableLocation { get; set; }

        /// <summary>
        ///     Gets or sets the registered application.
        /// </summary>
        /// <value>
        ///     The registered application.
        /// </value>
        public RegisteredApplication RegisteredApplication { get; set; }

        /// <summary>
        ///     Gets or sets the image source.
        /// </summary>
        /// <value>
        ///     The image source.
        /// </value>
        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
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
        /// <returns>A task, that when complete, will signal the completion of the loading of the image source</returns>
        public Task LoadIcon(IconSize iconSize)
        {
            return Task.Run(() => { ImageSource = IconHelper.IconFromFilePath(ExecutableLocation, iconSize); });
        }

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}