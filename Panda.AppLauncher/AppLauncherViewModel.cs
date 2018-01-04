using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Common.Logging;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     View model for the application launcher
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.IDisposable" />
    public sealed class AppLauncherViewModel : INotifyPropertyChanged, IDisposable
    {
        private IObservable<RegisteredApplicationViewModel> _previewDoubleClickObs;

        private IDisposable _previewDoubleClickSubscription;
        private IObservable<KeyEventArgs> _previewKeyUpObs;

        private IDisposable _previewKeyUpSubscription;
        private IObservable<string> _searchTextChangedObs;

        private IDisposable _textChangedSubscription;
        private IObservable<(RegisteredApplicationViewModel, MouseButtonEventArgs)> _previewMouseDoubleClickObs;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AppLauncherViewModel" /> class.
        /// </summary>
        /// <param name="registeredApplicationService">The registered application service.</param>
        /// <param name="registeredApplicationContextMenuProviders">The registered application context menu providers.</param>        
        public AppLauncherViewModel(
            IRegisteredApplicationService registeredApplicationService,
            IRegisteredApplicationContextMenuProvider[] registeredApplicationContextMenuProviders)
        {
            RegisteredApplicationService = registeredApplicationService;
            RegisteredApplicationContextMenuProviders = registeredApplicationContextMenuProviders;
        }

        public IObservable<RegisteredApplicationViewModel> PreviewDoubleClickObs
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
                        var filteredApps = RegisteredApplicationService.Get().Where(application =>
                            Regex.IsMatch(application.DisplayName, s, RegexOptions.IgnoreCase) ||
                            Regex.IsMatch(application.FullPath, s, RegexOptions.IgnoreCase)).ToEnumerable();
                        foreach (var registeredApplication in filteredApps)
                        {
                            var item = new RegisteredApplicationViewModel
                            {
                                AppName = registeredApplication.DisplayName,
                                ExecutableLocation = registeredApplication.FullPath,
                                RegisteredApplication = registeredApplication
                            };
                            AppViewModels.Add(item);
                            await item.LoadIcon();
                        }
                    });
            }
        }

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
        internal IRegisteredApplicationContextMenuProvider[] RegisteredApplicationContextMenuProviders { get; set; }

        /// <summary>
        ///     Gets or sets the application view models.
        /// </summary>
        /// <value>
        ///     The application view models.
        /// </value>
        public ObservableCollection<RegisteredApplicationViewModel> AppViewModels { get; set; } =
            new ObservableCollection<RegisteredApplicationViewModel>();

        /// <summary>
        ///     Gets the registered application service.
        /// </summary>
        /// <value>
        ///     The registered application service.
        /// </value>
        internal IRegisteredApplicationService RegisteredApplicationService { get; }

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

        private ILog Log { get; set; } = LogManager.GetLogger<AppLauncherViewModel>();

        private IDisposable _previewMouseUpSubscription;
        public IObservable<(RegisteredApplicationViewModel, MouseButtonEventArgs)> PreviewMouseDoubleClickObs
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

        public void Setup()
        {
            RegisteredApplicationService.Get().Subscribe(async application =>
            {
                var item = new RegisteredApplicationViewModel
                {
                    AppName = application.DisplayName,
                    ExecutableLocation = application.FullPath,
                    RegisteredApplication = application
                };
                AppViewModels.Add(item);
                await item.LoadIcon();
            });

            ApplicationRegisteredSubscription =
                RegisteredApplicationService.ApplicationRegisteredObservable.Subscribe(async application =>
                {
                    var item = new RegisteredApplicationViewModel
                    {
                        AppName = application.DisplayName,
                        ExecutableLocation = application.FullPath,
                        RegisteredApplication = application
                    };
                    AppViewModels.Add(item);
                    await item.LoadIcon();
                });

            ApplicationUnregisteredSubscription =
                RegisteredApplicationService.ApplicationUnregisteredObservable.Subscribe(application =>
                {
                    var toRemove = AppViewModels.Where(vm => vm.RegisteredApplication.Equals(application)).ToList();
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
        ///     Handles the selected items changed event
        /// </summary>
        /// <param name="selectedItems">The currently selected items.</param>
        internal void
            HandleSelectedItemsChanged(
                IEnumerable<RegisteredApplicationViewModel> selectedItems) // todo: replace with subscription
        {
            ContextMenuItems.Clear();
            var list = selectedItems.ToList();
            foreach (var registeredApplicationContextMenuProvider in RegisteredApplicationContextMenuProviders)
            {
                var canHandle =
                    registeredApplicationContextMenuProvider.CanHandle(
                        list.Select(model => model.RegisteredApplication));

                if (!canHandle) continue;

                foreach (var item in registeredApplicationContextMenuProvider.GetContextMenuItems(
                    list.Select(model => model.RegisteredApplication)))
                    ContextMenuItems.Add(item);
            }
        }
    }
}