using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    public class EverythingLauncherViewModel : INotifyPropertyChanged
    {
        public EverythingLauncherViewModel(EverythingService everythingService, IFileSystemContextMenuProvider[] fileSystemContextMenuProviders, IObservable<string> textChangedObservable)
        {
            EverythingService = everythingService;
            FileSystemContextMenuProviders = fileSystemContextMenuProviders;
            TextChangedObservable = textChangedObservable
                .ObserveOn(SynchronizationContext.Current)
                .Where(s => s != null && s.Length > 1)
                .Subscribe(HandleSearchTextChanged);
        }

        public IDisposable TextChangedObservable { get; set; }

        public IFileSystemContextMenuProvider[] FileSystemContextMenuProviders { get; set; }
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
            var fileInfos = selectedItems.Select(s => new FileInfo(s.FullName));
            var providers = FileSystemContextMenuProviders.Where(f => f.CanHandle(fileInfos));
            var menuItems = providers.SelectMany(p => p.GetContextMenuItems(fileInfos));
            foreach (var frameworkElement in menuItems)
            {
                ContextMenuItems.Add(frameworkElement);
            }
        }

        public void HandleSearchTextChanged(string newText)
        {
            CancellationTokenSource?.Cancel();
            CancellationTokenSource = new CancellationTokenSource();
            Subscription?.Dispose();
            EverythingResults.Clear();
            Subscription = EverythingService.Search(newText, CancellationTokenSource.Token)             
                .Subscribe(
                result =>
                {
                    var resultVm = new EverythingResultViewModel
                    {
                        FullName = result.FullPath
                    };
                    Application.Current.Dispatcher.Invoke(() => { EverythingResults.Add(resultVm); });
                });
        }
    }
}