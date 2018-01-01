using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     View model for the application launcher
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.IDisposable" />
    public sealed class AppLauncherViewModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AppLauncherViewModel" /> class.
        /// </summary>
        /// <param name="registeredApplicationService">The registered application service.</param>
        /// <param name="registeredApplicationContextMenuProviders">The registered application context menu providers.</param>
        public AppLauncherViewModel(IRegisteredApplicationService registeredApplicationService,
            IRegisteredApplicationContextMenuProvider[] registeredApplicationContextMenuProviders)
        {
            RegisteredApplicationService = registeredApplicationService;
            RegisteredApplicationContextMenuProviders = registeredApplicationContextMenuProviders;
            registeredApplicationService.Get().Subscribe(async application =>
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
                registeredApplicationService.ApplicationRegisteredObservable.Subscribe(async application =>
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
                registeredApplicationService.ApplicationUnregisteredObservable.Subscribe(application =>
                {
                    var toRemove = AppViewModels.Where(vm => vm.RegisteredApplication.Equals(application)).ToList();
                    foreach (var appViewModel in toRemove)
                        AppViewModels.Remove(appViewModel);
                });
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
        internal void HandleSelectedItemsChanged(IEnumerable<RegisteredApplicationViewModel> selectedItems)
        {
            ContextMenuItems.Clear();
            var list = selectedItems.ToList();
            foreach (var registeredApplicationContextMenuProvider in RegisteredApplicationContextMenuProviders)
            {
                var canHandle =
                    registeredApplicationContextMenuProvider.CanHandle(
                        list.Select(model => model.RegisteredApplication));
                if (canHandle)
                    foreach (var item in registeredApplicationContextMenuProvider.GetContextMenuItems(
                        list.Select(model => model.RegisteredApplication)))
                        ContextMenuItems.Add(item);
            }
        }
    }
}