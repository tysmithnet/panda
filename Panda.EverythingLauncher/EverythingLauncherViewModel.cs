﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Panda.EverythingLauncher
{
    public class EverythingLauncherViewModel : INotifyPropertyChanged
    {
        public string SearchText { get; set; }

        public ObservableCollection<EverythingResultViewModel> EverythingResults { get; set; } =
            new ObservableCollection<EverythingResultViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}