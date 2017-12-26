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
        }

        public ObservableCollection<AppViewModel> AppViewModels { get; set; }
        public AppLauncherRepository AppLauncherRepository { get; }
        public SettingsService SettingsService { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
