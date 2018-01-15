using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        ///     The UI scheduler
        /// </summary>
        private readonly IScheduler _uiScheduler;

        /// <summary>
        ///     The application name
        /// </summary>
        private string _appName;

        /// <summary>
        ///     The edit menu item
        /// </summary>
        private MenuItem _editMenuItem;

        /// <summary>
        ///     The executable location
        /// </summary>
        private string _executableLocation;

        /// <summary>
        ///     The image source for the icon
        /// </summary>
        private ImageSource _imageSource;

        /// <summary>
        ///     The is editable
        /// </summary>
        private bool _isEditable;

        /// <summary>
        ///     The launchable application
        /// </summary>
        private LaunchableApplication _launchableApplication;

        /// <summary>
        ///     The save menu item
        /// </summary>
        private MenuItem _saveMenuItem;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LaunchableApplicationViewModel" /> class.
        /// </summary>
        /// <param name="launcherService">The launcher service.</param>
        /// <exception cref="ArgumentNullException">launcherService</exception>
        public LaunchableApplicationViewModel(IScheduler uiScheduler, ILaunchableApplicationService launcherService)
        {
            _uiScheduler = uiScheduler;
            LaunchableApplicationService = launcherService ?? throw new ArgumentNullException(nameof(launcherService));
            SetupMenuItems();
            MenuItems.Add(_editMenuItem);
            Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(handler =>
                    {
                        PropertyChangedEventHandler h = (sender, args) => handler(args);
                        return h;
                    },
                    h => { PropertyChanged += h; },
                    h => { PropertyChanged -= h; })
                .SubscribeOn(TaskPoolScheduler.Default)
                .Throttle(TimeSpan.FromSeconds(5))
                .ObserveOn(_uiScheduler)
                .Subscribe(args => { LaunchableApplicationService.Save(); });
        }

        /// <summary>
        ///     Gets or sets the name of the application.
        /// </summary>
        /// <value>
        ///     The name of the application.
        /// </value>
        public string AppName
        {
            get => _appName;
            set
            {
                OnPropertyChanged();
                _appName = value;
                if (_launchableApplication != null)
                    _launchableApplication.DisplayName = value;
            }
        }

        /// <summary>
        ///     Gets or sets the executable location.
        /// </summary>
        /// <value>
        ///     The executable location.
        /// </value>
        public string ExecutableLocation
        {
            get => _executableLocation;
            set
            {
                OnPropertyChanged();
                _executableLocation = value;
                if (_launchableApplication != null)
                    _launchableApplication.FullPath = value;
            }
        }

        /// <summary>
        ///     Gets or sets the registered application.
        /// </summary>
        /// <value>
        ///     The registered application.
        /// </value>
        public LaunchableApplication LaunchableApplication
        {
            get => _launchableApplication;
            set
            {
                OnPropertyChanged();
                _launchableApplication = value;
                AppName = value?.DisplayName;
                ExecutableLocation = value?.FullPath;
            }
        }

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
        ///     Gets or sets a value indicating whether this instance is editable.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is editable; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                _isEditable = value;
                if (value)
                {
                    if (MenuItems.Contains(_editMenuItem))
                        MenuItems.Remove(_editMenuItem);
                    MenuItems.Add(_saveMenuItem);
                }
                else
                {
                    if (MenuItems.Contains(_saveMenuItem))
                        MenuItems.Remove(_saveMenuItem);
                    MenuItems.Add(_editMenuItem);
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the menu items.
        /// </summary>
        /// <value>
        ///     The menu items.
        /// </value>
        public ObservableCollection<FrameworkElement> MenuItems { get; set; } =
            new ObservableCollection<FrameworkElement>();

        /// <summary>
        ///     Gets or sets the launchable application service.
        /// </summary>
        /// <value>
        ///     The launchable application service.
        /// </value>
        internal ILaunchableApplicationService LaunchableApplicationService { get; set; }

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
            return Task.Run(() => { ImageSource = ShellHelper.IconFromFilePath(ExecutableLocation, iconSize); });
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
            _editMenuItem = new MenuItem {Header = "Edit"};
            _editMenuItem.Click += (sender, args) => { IsEditable = true; };

            _saveMenuItem = new MenuItem {Header = "Save"};
            _saveMenuItem.Click += (sender, args) => { IsEditable = false; };
        }
    }
}