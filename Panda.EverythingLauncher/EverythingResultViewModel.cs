using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Common.Logging;
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
            IsDirectory = System.IO.Directory.Exists(FullName);
            Name = Path.GetFileName(FullName);
            Directory = Path.GetDirectoryName(FullName);
            IsDirectory = System.IO.Directory.Exists(FullName);
            if (IsDirectory)
                return;
            try
            {
                FileInfo = new FileInfo(FullName);
                Size = FileInfo.Length;
                CreationTimeUtc = FileInfo.CreationTimeUtc;
                ModifiedTimeUtc = FileInfo.LastWriteTimeUtc;
            }
            catch (Exception e)
            {
                Log.Warn($"Problem loading file info for {fullName} - {e.Message}");
            }
        }

        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>
        ///     The log.
        /// </value>
        private ILog Log { get; } = LogManager.GetLogger<EverythingResultViewModel>();

        /// <summary>
        ///     Gets or sets the modified time UTC.
        /// </summary>
        /// <value>
        ///     The modified time UTC.
        /// </value>
        public DateTime? ModifiedTimeUtc { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is directory.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is directory; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirectory { get; }

        /// <summary>
        ///     Gets or sets the creation time UTC.
        /// </summary>
        /// <value>
        ///     The creation time UTC.
        /// </value>
        public DateTime? CreationTimeUtc { get; }

        /// <summary>
        ///     Gets or sets the file information.
        /// </summary>
        /// <value>
        ///     The file information.
        /// </value>
        public FileInfo FileInfo { get; }

        /// <summary>
        ///     Gets or sets the full name.
        /// </summary>
        /// <value>
        ///     The full name.
        /// </value>
        public string FullName { get; }

        /// <summary>
        ///     Gets or sets the directory.
        /// </summary>
        /// <value>
        ///     The directory.
        /// </value>
        public string Directory { get; }

        /// <summary>
        ///     Gets the size.
        /// </summary>
        /// <value>
        ///     The size.
        /// </value>
        public long? Size { get; }

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
        /// <returns>A task, that when complete, will signal the completion of the icon load</returns>
        public Task LoadIcon(uint retries = 0)
        {
            return Task.Run(async () =>
            {
                for (var i = 0; i < retries + 1; i++)
                    try
                    {
                        Icon = ShellHelper.IconFromFilePath(FullName, IconSize.Small);
                        return;
                    }
                    catch (Exception)
                    {
                        var timeoutMs = 1000; // todo: make setting
                        await Task.Delay(timeoutMs);
                        Icon = ShellHelper.GetFallbackIcon(IconSize.Small);
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