using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Panda.AppLauncher
{
    public class AppLauncherViewModel : INotifyPropertyChanged
    {
        public AppLauncherViewModel(RegisteredApplicationRepository registeredApplicationRepository,
            IRegisteredApplicationContextMenuProvider[] registeredApplicationContextMenuProviders)
        {
            RegisteredApplicationRepository = registeredApplicationRepository;
            RegisteredApplicationContextMenuProviders = registeredApplicationContextMenuProviders;
            foreach (var registeredApplication in registeredApplicationRepository.Get())
                AppViewModels.Add(new AppViewModel
                {
                    AppName = registeredApplication.DisplayName,
                    ExecutableLocation = registeredApplication.FullPath,
                    RegisteredApplication = registeredApplication
                });
            registeredApplicationRepository.ApplicationRegisteredObservable.Subscribe(application =>
            {
                AppViewModels.Add(new AppViewModel
                {
                    AppName = application.DisplayName,
                    ExecutableLocation = application.FullPath,
                    RegisteredApplication = application
                });
            });
            registeredApplicationRepository.ApplicationUnregisteredObservable.Subscribe(application =>
            {
                var toRemove = AppViewModels.Where(vm => vm.RegisteredApplication.Equals(application)).ToList();
                foreach (var appViewModel in toRemove)
                    AppViewModels.Remove(appViewModel);
            });
        }

        protected internal IRegisteredApplicationContextMenuProvider[] RegisteredApplicationContextMenuProviders { get; set; }

        protected internal ObservableCollection<AppViewModel> AppViewModels { get; set; } =
            new ObservableCollection<AppViewModel>();

        protected internal RegisteredApplicationRepository RegisteredApplicationRepository { get; }

        protected internal ObservableCollection<FrameworkElement> ContextMenuItems { get; set; } =
            new ObservableCollection<FrameworkElement>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected internal void HandleSelectedItemsChanged(IEnumerable<AppViewModel> selectedItems)
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