using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Panda.Client;

namespace Panda.AppLauncher
{
    public class AppLauncherViewModel : INotifyPropertyChanged
    {
        public AppLauncherViewModel(AppLauncherRepository appLauncherRepository)
        {
            AppLauncherRepository = appLauncherRepository;
            foreach (var registeredApplication in appLauncherRepository.Get())
            {
                AppViewModels.Add(new AppViewModel
                {
                    AppName = registeredApplication.DisplayName,
                    ExecutableLocation = registeredApplication.FullPath
                });
            }
        }

        public ObservableCollection<AppViewModel> AppViewModels { get; set; } = new ObservableCollection<AppViewModel>();
        public AppLauncherRepository AppLauncherRepository { get; }
        

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
