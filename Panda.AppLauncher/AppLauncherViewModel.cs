using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Panda.Client;

namespace Panda.AppLauncher
{
    public class AppLauncherViewModel : INotifyPropertyChanged
    {
        public AppLauncherViewModel(AppLauncherRepository appLauncherRepository, IRegisteredApplicationContextMenuProvider[] registeredApplicationContextMenuProviders)
        {
            AppLauncherRepository = appLauncherRepository;
            RegisteredApplicationContextMenuProviders = registeredApplicationContextMenuProviders;
            foreach (var registeredApplication in appLauncherRepository.Get())
            {
                AppViewModels.Add(new AppViewModel
                {
                    AppName = registeredApplication.DisplayName,
                    ExecutableLocation = registeredApplication.FullPath,
                    RegisteredApplication = registeredApplication
                });
            }
        }

        public IRegisteredApplicationContextMenuProvider[] RegisteredApplicationContextMenuProviders { get; set; }
        public ObservableCollection<AppViewModel> AppViewModels { get; set; } = new ObservableCollection<AppViewModel>();
        public AppLauncherRepository AppLauncherRepository { get; }
        public ObservableCollection<FrameworkElement> ContextMenuItems { get; set; } = new ObservableCollection<FrameworkElement>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void HandleSelectedItemsChanged(IEnumerable<AppViewModel> selectedItems)
        {
            ContextMenuItems.Clear(); 
            var list = selectedItems.ToList();
            foreach (var registeredApplicationContextMenuProvider in RegisteredApplicationContextMenuProviders)
            {
                var canHandle = registeredApplicationContextMenuProvider.CanHandle(list.Select(model => model.RegisteredApplication));
                if(canHandle)
                    foreach (var item in registeredApplicationContextMenuProvider.GetContextMenuItems(list.Select(model => model.RegisteredApplication)))
                    {
                        ContextMenuItems.Add(item);
                    }
            }
        }
    }
}
