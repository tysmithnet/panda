using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Common.Logging;
using Panda.Client;

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
        /// <param name="launchableApplicationService">The registered application service.</param>
        /// <param name="launchableApplicationContextMenuProviders">The registered application context menu providers.</param>
        public ApplicationLauncherViewModel(
            ILaunchableApplicationService launchableApplicationService,
            ILaunchableApplicationContextMenuProvider[] launchableApplicationContextMenuProviders)
        {
            LaunchableApplicationService = launchableApplicationService;
            LaunchableApplicationContextMenuProviders = launchableApplicationContextMenuProviders;
        }

        /// <summary>
        ///     Gets or sets the preview double click observable
        /// </summary>
        /// <value>
        ///     The preview double click observable.
        /// </value>
        public IObservable<LaunchableApplicationViewModel> PreviewDoubleClickObs
        {
            get => _previewDoubleClickObs;
            set
            {
                _previewDoubleClickSubscription?.Dispose();
                _previewDoubleClickObs = value;
                _previewDoubleClickSubscription = value.Subscribe(model =>
                {
                    Log.Trace($"Starting {model.ExecutableLocation}");
                    Process.Start(model.ExecutableLocation);
                });
            }
        }

        /// <summary>
        ///     Gets or sets the preview key up observable.
        /// </summary>
        /// <value>
        ///     The preview key up observable.
        /// </value>
        public IObservable<KeyEventArgs> PreviewKeyUpObs
        {
            get => _previewKeyUpObs;
            set
            {
                _previewKeyUpSubscription?.Dispose();
                _previewKeyUpObs = value;
                _previewKeyUpSubscription = value.Subscribe(args =>
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
        /// <value>
        ///     The search text changed observable.
        /// </value>
        /// <exception cref="ArgumentNullException">value</exception>
        public IObservable<string> SearchTextChangedObs
        {
            get => _searchTextChangedObs;
            set
            {
                value = value ?? throw new ArgumentNullException(nameof(value));
                _textChangedSubscription?.Dispose();
                _searchTextChangedObs = value;
                _textChangedSubscription = value.Where(s => s != null)
                    .Subscribe(async s =>
                    {
                        AppViewModels.Clear();
                        var filteredApps = LaunchableApplicationService.Get().Where(application =>
                            Regex.IsMatch(application.DisplayName, s, RegexOptions.IgnoreCase) ||
                            Regex.IsMatch(application.FullPath, s, RegexOptions.IgnoreCase)).ToEnumerable();
                        foreach (var registeredApplication in filteredApps)
                        {
                            var item = new LaunchableApplicationViewModel(LauncherService)
                            {
                                AppName = registeredApplication.DisplayName,
                                ExecutableLocation = registeredApplication.FullPath,
                                LaunchableApplication = registeredApplication
                            };
                            AppViewModels.Add(item);
                            await item.LoadIcon(IconSize.Large);
                        }
                    });
            }
        }

        internal ILaunchableApplicationService LauncherService { get; set; }

        /// <summary>
        ///     Gets or sets the application unregistered subscription.
        /// </summary>
        /// <value>
        ///     The application unregistered subscription.
        /// </value>
        internal IDisposable ApplicationUnregisteredSubscription { get; set; }

        /// <summary>
        ///     Gets or sets the application registered subscription.
        /// </summary>
        /// <value>
        ///     The application registered subscription.
        /// </value>
        internal IDisposable ApplicationRegisteredSubscription { get; set; }

        /// <summary>
        ///     Gets or sets the registered application context menu providers.
        /// </summary>
        /// <value>
        ///     The registered application context menu providers.
        /// </value>
        internal ILaunchableApplicationContextMenuProvider[] LaunchableApplicationContextMenuProviders { get; set; }

        /// <summary>
        ///     Gets or sets the application view models.
        /// </summary>
        /// <value>
        ///     The application view models.
        /// </value>
        public ObservableCollection<LaunchableApplicationViewModel> AppViewModels { get; set; } =
            new ObservableCollection<LaunchableApplicationViewModel>();

        /// <summary>
        ///     Gets the registered application service.
        /// </summary>
        /// <value>
        ///     The registered application service.
        /// </value>
        internal ILaunchableApplicationService LaunchableApplicationService { get; }

        /// <summary>
        ///     Gets or sets the context menu items.
        /// </summary>
        /// <value>
        ///     The context menu items.
        /// </value>
        public ObservableCollection<FrameworkElement> ContextMenuItems { get; set; } =
            new ObservableCollection<FrameworkElement>();

        /// <summary>
        ///     Gets or sets the search text.
        /// </summary>
        /// <value>
        ///     The search text.
        /// </value>
        public string SearchText { get; set; }

        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>
        ///     The log.
        /// </value>
        private ILog Log { get; } = LogManager.GetLogger<ApplicationLauncherViewModel>();

        /// <summary>
        ///     Gets or sets the preview mouse double click observable.
        /// </summary>
        /// <value>
        ///     The preview mouse double click observable.
        /// </value>
        public IObservable<(LaunchableApplicationViewModel, MouseButtonEventArgs)> PreviewMouseDoubleClickObs
        {
            get => _previewMouseDoubleClickObs;
            set
            {
                _previewMouseUpSubscription?.Dispose();
                _previewMouseDoubleClickObs = value;
                _previewMouseUpSubscription = value.Subscribe(tuple =>
                {
                    Log.Trace($"Starting {tuple.Item1.ExecutableLocation}");
                    Process.Start(tuple.Item1.ExecutableLocation);
                });
            }
        }

        /// <summary>
        ///     Gets or sets the selected items changed observable.
        /// </summary>
        /// <value>
        ///     The selected items changed observable.
        /// </value>
        public IObservable<IEnumerable<LaunchableApplicationViewModel>> SelectedItemsChangedObs
        {
            get => _selectedItemsChangedObs;
            set
            {
                _selectedItemsChangedSubscription?.Dispose();
                _selectedItemsChangedObs = value;
                _selectedItemsChangedSubscription = value.Subscribe(models =>
                {
                    ContextMenuItems.Clear();
                    var list = models.ToList();
                    foreach (var registeredApplicationContextMenuProvider in LaunchableApplicationContextMenuProviders)
                    {
                        var canHandle =
                            registeredApplicationContextMenuProvider.CanHandle(
                                list.Select(model => model.LaunchableApplication));

                        if (!canHandle) continue;

                        foreach (var item in registeredApplicationContextMenuProvider.GetContextMenuItems(
                            list.Select(model => model.LaunchableApplication)))
                            ContextMenuItems.Add(item);
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
        public void SetupSubscriptions()
        {
            LaunchableApplicationService.Get().Subscribe(async application =>
            {
                var item = new LaunchableApplicationViewModel(LaunchableApplicationService)
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
                LaunchableApplicationService.ApplicationRegisteredObservable.Subscribe(async application =>
                {
                    var item = new LaunchableApplicationViewModel(LaunchableApplicationService)
                    {
                        AppName = application.DisplayName,
                        ExecutableLocation = application.FullPath,
                        LaunchableApplication = application
                    };
                    AppViewModels.Add(item);
                    await item.LoadIcon(IconSize.Large);
                });

            ApplicationUnregisteredSubscription =
                LaunchableApplicationService.ApplicationUnregisteredObservable.Subscribe(application =>
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
    }
}