using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;

namespace Panda.Client
{
    public class LauncherSelectorViewModel : INotifyPropertyChanged
    {
        public LauncherSelectorViewModel(LauncherRepository launcherRepository,
            KeyboardMouseHookService keyboardMouseHookService, IObservable<string> textChangedObservable)
        {
            LauncherRepository = launcherRepository;
            ViewModels = LauncherRepository.Get().Select(l => new LauncherViewModel
            {
                Name = l.GetType().FullName,
                Instance = l
            });
            LauncherViewModels = new ObservableCollection<LauncherViewModel>(ViewModels);
            textChangedObservable
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(FilterApps);
        }

        protected internal LauncherRepository LauncherRepository { get; set; }

        protected internal IEnumerable<LauncherViewModel> ViewModels { get; set; }

        protected internal Launcher Active { get; set; }
        protected internal ObservableCollection<LauncherViewModel> LauncherViewModels { get; set; }

        protected internal string SearchText { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Handle(SelectionChangedEventArgs e)
        {
            var added = e.AddedItems.Cast<LauncherViewModel>();
            var launcherViewModels = added as LauncherViewModel[] ?? added.ToArray();
            if (launcherViewModels.Any())
            {
                var first = launcherViewModels.First();
                Active?.Hide();
                Active = first.Instance;
                Active.Show();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void FilterApps(string filter)
        {
            LauncherViewModels.Clear();
            foreach (var launcherViewModel in ViewModels.Where(vm =>
                Regex.IsMatch(vm.Name, filter, RegexOptions.IgnoreCase)))
                LauncherViewModels.Add(launcherViewModel);
        }

        public void Submit()
        {
            var first = LauncherViewModels.First();
            Active?.Hide();
            Active = first.Instance;
            Active.Show();
        }
    }
}