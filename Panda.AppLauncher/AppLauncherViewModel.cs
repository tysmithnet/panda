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

namespace Panda.AppLauncher
{
    /// <summary>
    ///     View model for the application launcher
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.IDisposable" />
    public sealed class AppLauncherViewModel : INotifyPropertyChanged, IDisposable
    {
        private IObservable<string> _textChangedObs;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AppLauncherViewModel" /> class.
        /// </summary>
        /// <param name="registeredApplicationService">The registered application service.</param>
        /// <param name="registeredApplicationContextMenuProviders">The registered application context menu providers.</param>
        /// <param name="textChangedObs"></param>
        /// <param name="previewKeyUpObs"></param>
        public AppLauncherViewModel(
            IRegisteredApplicationService registeredApplicationService, 
            IRegisteredApplicationContextMenuProvider[] registeredApplicationContextMenuProviders)
        {
            RegisteredApplicationService = registeredApplicationService;
            RegisteredApplicationContextMenuProviders = registeredApplicationContextMenuProviders;
        }

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

        private IDisposable _previewDoubleClickSubscription;
        public IObservable<RegisteredApplicationViewModel> PreviewDoubleClickObs
        {
            get => _previewDoubleClickObs;
            set
            {
                _previewDoubleClickSubscription?.Dispose();
                _previewDoubleClickObs = value;
                _previewDoubleClickSubscription = value.Subscribe(model =>
                {
                    Process.Start(model.ExecutableLocation);
                });
            }
        }

        private IDisposable _previewKeyUpSubscription;
        public IObservable<KeyEventArgs> PreviewKeyUpObs
        {
            get { return _previewKeyUpObs; }
            set
            {
                _previewDoubleClickSubscription.Dispose();
                _previewKeyUpObs = value; 
                _previewDoubleClickSubscription = value.Subscribe(args =>
                {
                    if (args.Key != Key.Enter && args.Key != Key.Return) return;
                    var first = AppViewModels.FirstOrDefault();
                    if (first != null)
                    {
                        Process.Start(first.ExecutableLocation);
                    }
                });
            }
        }

        private IDisposable _textChangedSubscription;
        private IObservable<RegisteredApplicationViewModel> _previewDoubleClickObs;
        private IObservable<KeyEventArgs> _previewKeyUpObs;

        public IObservable<string> TextChangedObs
        {
            get => _textChangedObs;
            set
            {
                value = value ?? throw new ArgumentNullException(nameof(value));
                _textChangedSubscription?.Dispose();                
                _textChangedObs = value; 
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
        /// Gets or sets the search text.
        /// </summary>
        /// <value>
        /// The search text.
        /// </value>
        public string SearchText { get; set; }
                                                      
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
        internal void HandleSelectedItemsChanged(IEnumerable<RegisteredApplicationViewModel> selectedItems) // todo: replace with subscription
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