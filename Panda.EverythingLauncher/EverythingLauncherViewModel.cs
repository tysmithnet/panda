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
using System.Windows.Input;
using Panda.Client;
using Panda.EverythingLauncher.Interop;
using Point = System.Drawing.Point;

namespace Panda.EverythingLauncher
{
    public class EverythingLauncherViewModel : INotifyPropertyChanged
    {
        public EverythingLauncherViewModel(EverythingService everythingService,
            IFileSystemContextMenuProvider[] fileSystemContextMenuProviders, IObservable<string> textChangedObservable,
            IObservable<IEnumerable<EverythingResultViewModel>> selectedItemsChangedObservable,
            IObservable<MouseButtonEventArgs> previewMouseRightButtonDownObservable)
        {
            EverythingService = everythingService;
            FileSystemContextMenuProviders = fileSystemContextMenuProviders;

            TextChangedSubscription = textChangedObservable
                .ObserveOn(SynchronizationContext.Current)
                .Where(s => s != null && s.Length > 1)
                .Subscribe(HandleSearchTextChanged);

            SelectedItemsChangedSubscription = selectedItemsChangedObservable
                .ObserveOn(SynchronizationContext.Current)
                .Where(model => model != null)
                .Subscribe(HandleSelectedResultsChanged);

            previewMouseRightButtonDownObservable
                .ObserveOn(SynchronizationContext.Current)
                .Where(args => args != null)
                .WithLatestFrom(selectedItemsChangedObservable,
                    (args, models) => (args, models))
                .Subscribe(tuple => HandlePreviewMouseRightButtonDown(tuple.Item1, tuple.Item2));
        }

        public IDisposable SelectedItemsChangedSubscription { get; set; }

        public IDisposable TextChangedSubscription { get; set; }
        public IFileSystemContextMenuProvider[] FileSystemContextMenuProviders { get; set; }
        public EverythingService EverythingService { get; set; }
        public string SearchText { get; set; }

        public ObservableCollection<EverythingResultViewModel> EverythingResults { get; set; } =
            new ObservableCollection<EverythingResultViewModel>();

        public ObservableCollection<FrameworkElement> ContextMenuItems { get; set; } =
            new ObservableCollection<FrameworkElement>();

        public CancellationTokenSource CancellationTokenSource { get; private set; }
        public IDisposable Subscription { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void HandleSelectedResultsChanged(IEnumerable<EverythingResultViewModel> selectedItems)
        {
            ContextMenuItems = new ObservableCollection<FrameworkElement>();
            var fileInfos = selectedItems.Select(s => new FileInfo(s.FullName));
            var providers = FileSystemContextMenuProviders.Where(f => f.CanHandle(fileInfos));
            var menuItems = providers.SelectMany(p => p.GetContextMenuItems(fileInfos));
            foreach (var frameworkElement in menuItems)
                ContextMenuItems.Add(frameworkElement);
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

        public void HandlePreviewMouseRightButtonDown(MouseButtonEventArgs mouseButtonEventArgs,
            IEnumerable<EverythingResultViewModel> selectedEverythingResultViewModels)
        {
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                var shellContextMenu = new ShellContextMenu();     
                var point = Mouse.GetPosition(null);
                var fileInfos = selectedEverythingResultViewModels.Select(vm => new FileInfo(vm.FullName)).ToList();
                var directories = fileInfos.Where(info => info.Attributes.HasFlag(FileAttributes.Directory)).ToList();
                var directoryInfos = directories.Select(info => info.Directory).ToArray();
                var files = fileInfos.Except(directories).ToArray();
                var pointInfo = new Point((int) point.X, (int) point.Y);
                if (!files.Any() && !directories.Any())
                    return;
                if(!files.Any())
                    shellContextMenu.ShowContextMenu(directoryInfos, pointInfo);
                else if(!directories.Any())
                    shellContextMenu.ShowContextMenu(files, pointInfo);
                else
                    shellContextMenu.ShowContextMenu(files, directoryInfos, pointInfo);
            }
        }
    }
}