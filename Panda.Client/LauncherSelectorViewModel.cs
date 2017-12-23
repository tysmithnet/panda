using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Panda.Client
{
    public class LauncherSelectorViewModel : INotifyPropertyChanged
    {
        public LauncherSelectorViewModel(IEnumerable<Launcher> launchers)
        {
            var viewModels = launchers.Select(l => new LauncherViewModel
            {
                Name = l.GetType().FullName,
                Instance = l
            });
            LauncherViewModels = new ObservableCollection<LauncherViewModel>(viewModels);
        }

        public ObservableCollection<LauncherViewModel> LauncherViewModels { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}