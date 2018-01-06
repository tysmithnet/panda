using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Common.Logging;
using Humanizer;
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

        private ILog Log { get; } = LogManager.GetLogger<EverythingResultViewModel>();

        public DateTime? ModifiedTimeUtc { get; set; }

        public bool IsDirectory { get; set; }

        public DateTime? CreationTimeUtc { get; set; }

        public FileInfo FileInfo { get; set; }

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

        public long? Size { get; }

        public string SizeHumanized
        {
            get
            {
                if (!Size.HasValue)
                    return "";
                return Humanizer.Bytes.ByteSize.FromBytes(Size.Value).Humanize("#.#");
            }
        }       

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
        public Task LoadIcon(uint retries = 0)
        {
            return Task.Run(async () =>
            {
                for (var i = 0; i < retries + 1; i++)
                    try
                    {
                        Icon = IconHelper.IconFromFilePath(FullName, IconSize.Small);
                        return;
                    }
                    catch (Exception)
                    {
                        var timeoutMs = 1000; // todo: make setting
                        await Task.Delay(timeoutMs);
                        Icon = IconHelper.GetFallbackIcon(IconSize.Small);
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