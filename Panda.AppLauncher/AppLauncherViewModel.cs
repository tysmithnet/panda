using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Panda.AppLauncher
{
    public sealed class AppLauncherViewModel : INotifyPropertyChanged, IDisposable
    {
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

        internal IDisposable ApplicationUnregisteredSubscription { get; set; }

        internal IDisposable ApplicationRegisteredSubscription { get; set; }

        internal IRegisteredApplicationContextMenuProvider[] RegisteredApplicationContextMenuProviders { get; set; }

        public ObservableCollection<RegisteredApplicationViewModel> AppViewModels { get; set; } =
            new ObservableCollection<RegisteredApplicationViewModel>();

        internal IRegisteredApplicationService RegisteredApplicationService { get; }

        public ObservableCollection<FrameworkElement> ContextMenuItems { get; set; } =
            new ObservableCollection<FrameworkElement>();

        public void Dispose()
        {
            ApplicationUnregisteredSubscription?.Dispose();
            ApplicationRegisteredSubscription?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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