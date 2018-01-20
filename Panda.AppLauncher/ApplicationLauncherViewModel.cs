using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Common.Logging;
using Microsoft.Win32;
using Panda.Client;
using Panda.Client.Interop;
using Point = System.Drawing.Point;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     View model for the application launcher
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.IDisposable" />
    public sealed class ApplicationLauncherViewModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        ///     The add application button clicked obs
        /// </summary>
        private IObservable<RoutedEventArgs> _addApplicationButtonClickedObs;

        /// <summary>
        ///     The add application button clicked subscription
        /// </summary>
        private IDisposable _addApplicationButtonClickedSubscription;

        /// <summary>
        ///     The mouse right button up obs
        /// </summary>
        private IObservable<(LaunchableApplicationViewModel, MouseButtonEventArgs)> _mouseRightButtonUpObs;

        /// <summary>
        ///     The mouse right button up subscription
        /// </summary>
        private IDisposable _mouseRightButtonUpSubscription;

        /// <summary>
        ///     The preview double click observable
        /// </summary>
        private IObservable<LaunchableApplicationViewModel> _previewDoubleClickObs;

        /// <summary>
        ///     The preview double click subscription
        /// </summary>
        private IDisposable _previewDoubleClickSubscription;

        /// <summary>
        ///     The preview key up observable
        /// </summary>
        private IObservable<KeyEventArgs> _previewKeyUpObs;

        /// <summary>
        ///     The preview key up subscription
        /// </summary>
        private IDisposable _previewKeyUpSubscription;

        /// <summary>
        ///     The preview mouse double click observable
        /// </summary>
        private IObservable<(LaunchableApplicationViewModel, MouseButtonEventArgs)> _previewMouseDoubleClickObs;

        /// <summary>
        ///     The preview mouse up subscription
        /// </summary>
        private IDisposable _previewMouseUpSubscription;

        /// <summary>
        ///     The search text changed observable
        /// </summary>
        private IObservable<string> _searchTextChangedObs;

        /// <summary>
        ///     The selected items changed observable
        /// </summary>
        private IObservable<IEnumerable<LaunchableApplicationViewModel>> _selectedItemsChangedObs;

        /// <summary>
        ///     The selected items changed subscription
        /// </summary>
        private IDisposable _selectedItemsChangedSubscription;

        /// <summary>
        ///     The text changed subscription
        /// </summary>
        private IDisposable _textChangedSubscription;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationLauncherViewModel" /> class.
        /// </summary>
        /// <param name="uiScheduler">The UI scheduler.</param>
        /// <param name="launchableApplicationService">The registered application service.</param>
        /// <param name="keyboardMouseService"></param>
        /// <param name="launchableApplicationContextMenuProviders">The registered application context menu providers.</param>
        public ApplicationLauncherViewModel(
            IScheduler uiScheduler,
            ILaunchableApplicationService launchableApplicationService,
            IKeyboardMouseService keyboardMouseService,
            ILaunchableApplicationContextMenuProvider[] launchableApplicationContextMenuProviders)
        {
            KeyboardMouseService = keyboardMouseService;
            UiScheduler = uiScheduler;
            LaunchableApplicationService = launchableApplicationService;
            LaunchableApplicationContextMenuProviders = launchableApplicationContextMenuProviders;
        }

        /// <summary>
        ///     Gets the keyboard mouse service.
        /// </summary>
        /// <value>The keyboard mouse service.</value>
        private IKeyboardMouseService KeyboardMouseService { get; }

        /// <summary>
        ///     Gets or sets the UI scheduler.
        /// </summary>
        /// <value>The UI scheduler.</value>
        private IScheduler UiScheduler { get; set; }

        /// <summary>
        ///     Gets or sets the preview double click observable
        /// </summary>
        /// <value>The preview double click observable.</value>
        internal IObservable<LaunchableApplicationViewModel> PreviewDoubleClickObs
        {
            get => _previewDoubleClickObs;
            set
            {
                _previewDoubleClickSubscription?.Dispose();
                _previewDoubleClickObs = value;
                _previewDoubleClickSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .Subscribe(model =>
                    {
                        Log.Trace($"Starting {model.ExecutableLocation}");
                        Process.Start(model.ExecutableLocation);
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the preview key up observable.
        /// </summary>
        /// <value>The preview key up observable.</value>
        internal IObservable<KeyEventArgs> PreviewKeyUpObs
        {
            get => _previewKeyUpObs;
            set
            {
                _previewKeyUpSubscription?.Dispose();
                _previewKeyUpObs = value;
                _previewKeyUpSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .Subscribe(args =>
                    {
                        if (args.Key != Key.Enter && args.Key != Key.Return) return;
                        var first = AppViewModels.FirstOrDefault();
                        if (first != null)
                        {
                            Log.Trace($"Starting {first.ExecutableLocation}");
                            Process.Start(first.ExecutableLocation);
                        }
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the search text changed observable.
        /// </summary>
        /// <value>The search text changed observable.</value>
        /// <exception cref="ArgumentNullException">value</exception>
        internal IObservable<string> SearchTextChangedObs
        {
            get => _searchTextChangedObs;
            set
            {
                value = value ?? throw new ArgumentNullException(nameof(value));
                _textChangedSubscription?.Dispose();
                _searchTextChangedObs = value;
                _textChangedSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
                    .Where(s => s != null)
                    .Subscribe(async s =>
                    {
                        AppViewModels.Clear();
                        var filteredApps = LaunchableApplicationService.Get().Where(application =>
                            Regex.IsMatch(application.DisplayName, s, RegexOptions.IgnoreCase) ||
                            Regex.IsMatch(application.FullPath, s, RegexOptions.IgnoreCase)).ToEnumerable();
                        foreach (var registeredApplication in filteredApps)
                        {
                            var item = new LaunchableApplicationViewModel(UiScheduler, LaunchableApplicationService)
                            {
                                AppName = registeredApplication.DisplayName,
                                ExecutableLocation = registeredApplication.FullPath,
                                LaunchableApplication = registeredApplication,
                                LaunchableApplicationService = LaunchableApplicationService
                            };
                            AppViewModels.Add(item);
                            await item.LoadIcon(IconSize.Large);
                        }
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the application unregistered subscription.
        /// </summary>
        /// <value>The application unregistered subscription.</value>
        internal IDisposable ApplicationUnregisteredSubscription { get; set; }

        /// <summary>
        ///     Gets or sets the application registered subscription.
        /// </summary>
        /// <value>The application registered subscription.</value>
        internal IDisposable ApplicationRegisteredSubscription { get; set; }

        /// <summary>
        ///     Gets or sets the registered application context menu providers.
        /// </summary>
        /// <value>The registered application context menu providers.</value>
        internal ILaunchableApplicationContextMenuProvider[] LaunchableApplicationContextMenuProviders { get; set; }

        /// <summary>
        ///     Gets or sets the application view models.
        /// </summary>
        /// <value>The application view models.</value>
        public ObservableCollection<LaunchableApplicationViewModel> AppViewModels { get; set; } =
            new ObservableCollection<LaunchableApplicationViewModel>();

        /// <summary>
        ///     Gets the registered application service.
        /// </summary>
        /// <value>The registered application service.</value>
        internal ILaunchableApplicationService LaunchableApplicationService { get; }

        /// <summary>
        ///     Gets or sets the context menu items.
        /// </summary>
        /// <value>The context menu items.</value>
        public ObservableCollection<FrameworkElement> ContextMenuItems { get; set; } =
            new ObservableCollection<FrameworkElement>();

        /// <summary>
        ///     Gets or sets the search text.
        /// </summary>
        /// <value>The search text.</value>
        public string SearchText { get; set; }

        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>The log.</value>
        private ILog Log { get; } = LogManager.GetLogger<ApplicationLauncherViewModel>();

        /// <summary>
        ///     Gets or sets the preview mouse double click observable.
        /// </summary>
        /// <value>The preview mouse double click observable.</value>
        internal IObservable<(LaunchableApplicationViewModel, MouseButtonEventArgs)> PreviewMouseDoubleClickObs
        {
            get => _previewMouseDoubleClickObs;
            set
            {
                _previewMouseUpSubscription?.Dispose();
                _previewMouseDoubleClickObs = value;
                _previewMouseUpSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .Subscribe(tuple =>
                    {
                        Log.Trace($"Starting {tuple.Item1.ExecutableLocation}");
                        Process.Start(tuple.Item1.ExecutableLocation);
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the selected items changed observable.
        /// </summary>
        /// <value>The selected items changed observable.</value>
        internal IObservable<IEnumerable<LaunchableApplicationViewModel>> SelectedItemsChangedObs
        {
            get => _selectedItemsChangedObs;
            set
            {
                _selectedItemsChangedSubscription?.Dispose();
                _selectedItemsChangedObs = value;
                _selectedItemsChangedSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
                    .Subscribe(models =>
                    {
                        ContextMenuItems.Clear();
                        var list = models.ToList();
                        foreach (var registeredApplicationContextMenuProvider in
                            LaunchableApplicationContextMenuProviders)
                        {
                            var canHandle =
                                registeredApplicationContextMenuProvider.CanHandle(
                                    list);

                            if (!canHandle) continue;

                            foreach (var item in registeredApplicationContextMenuProvider.GetContextMenuItems(
                                list))
                                ContextMenuItems.Add(item);
                        }
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the add application button clicked obs.
        /// </summary>
        /// <value>The add application button clicked obs.</value>
        internal IObservable<RoutedEventArgs> AddApplicationButtonClickedObs
        {
            get => _addApplicationButtonClickedObs;
            set
            {
                _addApplicationButtonClickedSubscription?.Dispose();
                _addApplicationButtonClickedObs = value;
                _addApplicationButtonClickedSubscription = value
                    .ObserveOn(UiScheduler)
                    .Subscribe(args =>
                    {
                        var dlg = new OpenFileDialog
                        {
                            DefaultExt = ".exe",
                            Filter = "EXE Files (*.exe)|*.exe|All Files (*.*)|*.*"
                        };

                        var result = dlg.ShowDialog();

                        if (result == true)
                        {
                            var filename = dlg.FileName;
                            LaunchableApplicationService.Add(filename);
                        }
                    });
            }
        }                                            

        /// <summary>
        ///     Gets or sets the mouse right button up obs.
        /// </summary>
        /// <value>The mouse right button up obs.</value>
        internal IObservable<(LaunchableApplicationViewModel, MouseButtonEventArgs)> MouseRightButtonUpObs
        {
            get => _mouseRightButtonUpObs;
            set
            {
                _mouseRightButtonUpSubscription?.Dispose();
                _mouseRightButtonUpObs = value;
                _mouseRightButtonUpSubscription = value.SubscribeOn(TaskPoolScheduler.Default).ObserveOn(UiScheduler)
                    .Subscribe(
                        tuple =>
                        {
                            try
                            {
                                if (KeyboardMouseService.IsKeyDown(Key.LeftAlt) ||
                                    KeyboardMouseService.IsKeyDown(Key.RightAlt))
                                {
                                    var shellContextMenu = new ShellContextMenu();
                                    var point = KeyboardMouseService.GetMousePosition();
                                    shellContextMenu.ShowContextMenu(
                                        new[] {new FileInfo(tuple.Item1.ExecutableLocation)},
                                        new Point((int) point.X, (int) point.Y));
                                }
                            }
                            catch (Exception)
                            {
                                Log.Error($"Unable to show shell context menu for {tuple.Item1.ToString()}");
                                throw;
                            }
                        });
            }
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            ApplicationUnregisteredSubscription?.Dispose();
            ApplicationRegisteredSubscription?.Dispose();
        }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Setups this instance's primary subscriptions
        /// </summary>
        internal void SetupSubscriptions()
        {
            LaunchableApplicationService.Get()
                .SubscribeOn(TaskPoolScheduler.Default)
                .ObserveOn(UiScheduler)
                .Subscribe(async application =>
                {
                    var item = new LaunchableApplicationViewModel(UiScheduler, LaunchableApplicationService)
                    {
                        AppName = application.DisplayName,
                        ExecutableLocation = application.FullPath,
                        LaunchableApplication = application,
                        LaunchableApplicationService = LaunchableApplicationService
                    };
                    AppViewModels.Add(item);
                    await item.LoadIcon(IconSize.Large);
                });

            ApplicationRegisteredSubscription =
                LaunchableApplicationService.ApplicationRegisteredObservable
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
                    .Subscribe(async application =>
                    {
                        var item = new LaunchableApplicationViewModel(UiScheduler, LaunchableApplicationService)
                        {
                            AppName = application.DisplayName,
                            ExecutableLocation = application.FullPath,
                            LaunchableApplication = application
                        };
                        AppViewModels.Add(item);
                        await item.LoadIcon(IconSize.Large);
                    });

            ApplicationUnregisteredSubscription =
                LaunchableApplicationService.ApplicationUnregisteredObservable
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
                    .Subscribe(application =>
                    {
                        var toRemove = AppViewModels.Where(vm => vm.LaunchableApplication.Equals(application)).ToList();
                        foreach (var appViewModel in toRemove)
                            AppViewModels.Remove(appViewModel);
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

        /// <summary>
        ///     Called when [activated].
        /// </summary>
        internal void OnActivated()
        {
            SearchText = "";
        }
    }
}