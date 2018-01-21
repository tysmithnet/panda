using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Common.Logging;
using Panda.Client;
using Panda.Client.Properties;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     View model for launchable applications
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public sealed class LaunchableApplicationViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     The application name
        /// </summary>
        private string _appName;

        /// <summary>
        ///     The executable location
        /// </summary>
        private string _executableLocation;

        /// <summary>
        ///     The image source for the icon
        /// </summary>
        private ImageSource _imageSource;

        /// <summary>
        ///     The launchable application
        /// </summary>
        private LaunchableApplication _instance;

        /// <summary>
        ///     The is editable
        /// </summary>
        private bool _isEditable;

        /// <summary>
        ///     The save menu item
        /// </summary>
        private MenuItem _saveMenuItem;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LaunchableApplicationViewModel" /> class.
        /// </summary>
        /// <param name="uiScheduler">The UI scheduler.</param>
        /// <param name="launcherService">The launcher service.</param>
        /// <exception cref="ArgumentNullException">launcherService</exception>
        public LaunchableApplicationViewModel(IScheduler uiScheduler, ILaunchableApplicationService launcherService)
        {
            UiScheduler = uiScheduler;
            LaunchableApplicationService = launcherService ?? throw new ArgumentNullException(nameof(launcherService));
            SetupMenuItems();
            MenuItems.Add(EditMenuItem);
            Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(handler =>
                    {
                        PropertyChangedEventHandler h = (sender, args) => handler(args);
                        return h;
                    },
                    h => { PropertyChanged += h; },
                    h => { PropertyChanged -= h; })
                .SubscribeOn(TaskPoolScheduler.Default)
                .Throttle(TimeSpan.FromSeconds(5))
                .ObserveOn(UiScheduler)
                .Subscribe(args => { LaunchableApplicationService.Save(); });
        }

        /// <summary>
        ///     The UI scheduler
        /// </summary>
        /// <value>The UI scheduler.</value>
        private IScheduler UiScheduler { get; }

        /// <summary>
        ///     The edit menu item
        /// </summary>
        /// <value>The edit menu item.</value>
        private MenuItem EditMenuItem { get; set; }

        /// <summary>
        ///     Gets or sets the name of the application.
        /// </summary>
        /// <value>The name of the application.</value>
        public string AppName
        {
            get => _appName;
            set
            {
                OnPropertyChanged();
                _appName = value;
                if (_instance != null)
                    _instance.DisplayName = value;
            }
        }

        /// <summary>
        ///     Gets or sets the executable location.
        /// </summary>
        /// <value>The executable location.</value>
        public string ExecutableLocation
        {
            get => _executableLocation;
            set
            {
                OnPropertyChanged();
                _executableLocation = value;
                if (_instance != null)
                    _instance.FullPath = value;
            }
        }

        /// <summary>
        ///     Gets or sets the registered application.
        /// </summary>
        /// <value>The registered application.</value>
        public LaunchableApplication Instance
        {
            get => _instance;
            set
            {
                OnPropertyChanged();
                _instance = value;
                AppName = value?.DisplayName;
                ExecutableLocation = value?.FullPath;
            }
        }

        /// <summary>
        ///     Gets or sets the image source.
        /// </summary>
        /// <value>The image source.</value>
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
        ///     Gets or sets a value indicating whether this instance is editable.
        /// </summary>
        /// <value><c>true</c> if this instance is editable; otherwise, <c>false</c>.</value>
        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                _isEditable = value;
                if (value)
                {
                    if (MenuItems.Contains(EditMenuItem))
                        MenuItems.Remove(EditMenuItem);
                    MenuItems.Add(_saveMenuItem);
                }
                else
                {
                    if (MenuItems.Contains(_saveMenuItem))
                        MenuItems.Remove(_saveMenuItem);
                    MenuItems.Add(EditMenuItem);
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the menu items.
        /// </summary>
        /// <value>The menu items.</value>
        public ObservableCollection<FrameworkElement> MenuItems { get; set; } =
            new ObservableCollection<FrameworkElement>();

        /// <summary>
        ///     Gets or sets the launchable application service.
        /// </summary>
        /// <value>The launchable application service.</value>
        internal ILaunchableApplicationService LaunchableApplicationService { get; set; }

        /// <summary>
        ///     Gets or sets the log.
        /// </summary>
        /// <value>The log.</value>
        private ILog Log { get; } = LogManager.GetLogger<LaunchableApplicationViewModel>();

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Loads the icon.
        /// </summary>
        /// <param name="iconSize">Size of the icon.</param>
        /// <returns>A task, that when complete, will signal the completion of the loading of the image source</returns>
        public Task LoadIcon(IconSize iconSize)
        {
            return Task.Run(() =>
            {
                try
                {
                    ImageSource = string.IsNullOrWhiteSpace(Instance.IconPath)
                        ? ShellHelper.IconFromFilePath(ExecutableLocation, iconSize)
                        : CreateImageSource(Instance.IconPath, iconSize);
                }
                catch (Exception e)
                {
                    Log.Error($"Unable to load icon for {ExecutableLocation} - {e.Message}");
                }
            });
        }

        /// <summary>
        ///     Creates the image source.
        /// </summary>
        /// <param name="iconPath">The icon path.</param>
        /// <param name="iconSize">Size of the icon.</param>
        /// <returns>ImageSource.</returns>
        private ImageSource CreateImageSource(string iconPath, IconSize iconSize)
        {
            var fileInfo = new FileInfo(iconPath);
            if (!new[] {"jpeg", "jpg", "png", "gif"}.Contains(fileInfo.Extension.ToLower()))
                return ShellHelper.IconFromFilePath(fileInfo.FullName, iconSize);
            BitmapImage bmi = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                bmi = new BitmapImage(new Uri(fileInfo.FullName, UriKind.Absolute));
            });

            return bmi;
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

        /// <summary>
        ///     Setups the menu items.
        /// </summary>
        private void SetupMenuItems()
        {
            EditMenuItem = new MenuItem {Header = "Edit"};
            EditMenuItem.Click += (sender, args) => { IsEditable = true; };

            _saveMenuItem = new MenuItem {Header = "Save"};
            _saveMenuItem.Click += (sender, args) => { IsEditable = false; };
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{AppName} - {ExecutableLocation}";
        }
    }
}