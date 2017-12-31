﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Panda.Client
{
    public class LauncherSelectorViewModel : INotifyPropertyChanged
    {
        public LauncherRepository LauncherRepository { get; set; }

        public LauncherSelectorViewModel(LauncherRepository launcherRepository, KeyboardMouseHookService keyboardMouseHookService ,IObservable<string> textChangedObservable)
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

        public IEnumerable<LauncherViewModel> ViewModels { get; set; }

        public ObservableCollection<LauncherViewModel> LauncherViewModels { get; set; }

        public string SearchText { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void FilterApps(string filter)
        {
            LauncherViewModels.Clear();
            foreach (var launcherViewModel in ViewModels.Where(vm => Regex.IsMatch(vm.Name, filter, RegexOptions.IgnoreCase)))
            {
                LauncherViewModels.Add(launcherViewModel);
            }
        }
    }
}