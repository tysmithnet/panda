using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Panda.EverythingLauncher
{
    public class EverythingLauncherViewModel : INotifyPropertyChanged
    {
        public EverythingLauncherViewModel(EverythingService everythingService)
        {
            EverythingService = everythingService;
        }

        public EverythingService EverythingService { get; set; }

        public string SearchText { get; set; }

        public ObservableCollection<EverythingResultViewModel> EverythingResults { get; set; } =
            new ObservableCollection<EverythingResultViewModel>();
                                                                    
        
        public ObservableCollection<FrameworkElement> ContextMenuItems { get; set; } = new ObservableCollection<FrameworkElement>();

        public CancellationTokenSource CancellationTokenSource { get; private set; }


        public IDisposable Subscription { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void HandleSelectedResultsChanged(IEnumerable<EverythingResultViewModel> selectedItems)
        {
            ContextMenuItems.Clear();
            foreach (var everythingResultViewModel in selectedItems)
            {
                var menuItem = new MenuItem
                {
                    Header = everythingResultViewModel.Name
                };
                ContextMenuItems.Add(menuItem);
            }
        }

        public void HandleSearchTextChanged(TextChangedEventArgs textChangedEventArgs)
        {
            CancellationTokenSource?.Cancel();
            CancellationTokenSource = new CancellationTokenSource();
            Subscription?.Dispose();
            EverythingResults.Clear();
            Subscription = EverythingService.Search(SearchText, CancellationTokenSource.Token).Subscribe(
                result =>
                {
                    var resultVm = new EverythingResultViewModel
                    {
                        Name = result.FullPath
                    };
                    Application.Current.Dispatcher.Invoke(() => { EverythingResults.Add(resultVm); });
                });
        }
    }
}