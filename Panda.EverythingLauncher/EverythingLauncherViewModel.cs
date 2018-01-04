using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Common.Logging;
using Panda.Client;
using Panda.EverythingLauncher.Interop;
using Point = System.Drawing.Point;

namespace Panda.EverythingLauncher
{
    /// <summary>
    ///     View model for the everything launcher
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public sealed class EverythingLauncherViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     The everything subscription
        /// </summary>
        private IDisposable _everythingSubscription;

        /// <summary>
        ///     The preview mouse right button down obs
        /// </summary>
        private IObservable<MouseButtonEventArgs> _previewMouseRightButtonDownObs;

        /// <summary>
        ///     The preview mouse right button down subscription
        /// </summary>
        private IDisposable _previewMouseRightButtonDownSubscription;

        /// <summary>
        ///     The selected items changed obs
        /// </summary>
        private IObservable<IEnumerable<EverythingResultViewModel>> _selectedItemsChangedObs;

        /// <summary>
        ///     The selected items changed subscription
        /// </summary>
        private IDisposable _selectedItemsChangedSubscription;

        /// <summary>
        ///     The text changed obs
        /// </summary>
        private IObservable<string> _textChangedObs;

        /// <summary>
        ///     The text changed subscription
        /// </summary>
        private IDisposable _textChangedSubscription;

        private IObservable<(EverythingResultViewModel, MouseButtonEventArgs)> _previewMouseDoubleClickObs;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EverythingLauncherViewModel" /> class.
        /// </summary>
        /// <param name="everythingService">The everything service.</param>
        /// <param name="fileSystemContextMenuProviders">The file system context menu providers.</param>
        public EverythingLauncherViewModel(EverythingService everythingService,
            IFileSystemContextMenuProvider[] fileSystemContextMenuProviders)
        {
            EverythingService = everythingService;
            FileSystemContextMenuProviders = fileSystemContextMenuProviders;
        }

        /// <summary>
        ///     Gets or sets the preview mouse right button down obs.
        /// </summary>
        /// <value>
        ///     The preview mouse right button down obs.
        /// </value>
        public IObservable<MouseButtonEventArgs> PreviewMouseRightButtonDownObs
        {
            get => _previewMouseRightButtonDownObs;
            set
            {
                _previewMouseRightButtonDownSubscription?.Dispose();
                _previewMouseRightButtonDownObs = value;
                _previewMouseRightButtonDownSubscription = value  
                    .Where(args => args != null)
                    .WithLatestFrom(SelectedItemsChangedObs,
                        (args, models) => (args, models))
                    .Subscribe(tuple =>
                    {
                        if (!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt)) return;
                        var shellContextMenu = new ShellContextMenu();
                        var point = Mouse.GetPosition(null);
                        var fileInfos = tuple.Item2.Select(vm => new FileInfo(vm.FullName)).ToList();
                        var directories = fileInfos.Where(info => info.Attributes.HasFlag(FileAttributes.Directory))
                            .ToList();
                        var directoryInfos = directories.Select(info => info.Directory).ToArray();
                        var files = fileInfos.Except(directories).ToArray();
                        var pointInfo = new Point((int) point.X, (int) point.Y);
                        if (!files.Any() && !directories.Any())
                            return;
                        if (!files.Any())
                            shellContextMenu.ShowContextMenu(directoryInfos, pointInfo);
                        else if (!directories.Any())
                            shellContextMenu.ShowContextMenu(files, pointInfo);
                        else
                            shellContextMenu.ShowContextMenu(files, directoryInfos, pointInfo);
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the selected items changed obs.
        /// </summary>
        /// <value>
        ///     The selected items changed obs.
        /// </value>
        public IObservable<IEnumerable<EverythingResultViewModel>> SelectedItemsChangedObs
        {
            get => _selectedItemsChangedObs;
            set
            {
                _selectedItemsChangedSubscription?.Dispose();
                _selectedItemsChangedObs = value;
                _selectedItemsChangedSubscription = value        
                    .Where(model => model != null)
                    .Subscribe(selectedItems =>
                    {
                        ContextMenuItems.Clear();
                        SelectedItems = selectedItems.ToList();
                        var fileInfos = SelectedItems.Select(s => new FileInfo(s.FullName));
                        var providers = FileSystemContextMenuProviders.Where(f => f.CanHandle(fileInfos));
                        var menuItems = providers.SelectMany(p => p.GetContextMenuItems(fileInfos));
                        foreach (var frameworkElement in menuItems)
                            ContextMenuItems.Add(frameworkElement);
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the text changed obs.
        /// </summary>
        /// <value>
        ///     The text changed obs.
        /// </value>
        public IObservable<string> TextChangedObs
        {
            get => _textChangedObs;
            set
            {
                _textChangedSubscription?.Dispose();
                _textChangedObs = value;
                _textChangedSubscription = value
                    .Where(s => s != null && s.Length > 1)
                    .Throttle(TimeSpan.FromMilliseconds(333))
                    .Subscribe(s =>
                    {
                        CancellationTokenSource?.Cancel();
                        CancellationTokenSource = new CancellationTokenSource();
                        _everythingSubscription?.Dispose();
                        Application.Current.Dispatcher.Invoke(() => EverythingResults.Clear());                        
                        _everythingSubscription = EverythingService
                            .Search(s, CancellationTokenSource.Token)
                            .ForEachAsync(
                                async result =>
                                {
                                    var resultVm = new EverythingResultViewModel(result.FullPath);
                                    Application.Current.Dispatcher.Invoke(() => { EverythingResults.Add(resultVm); });
                                    await resultVm.LoadIcon();
                                }, CancellationTokenSource.Token);
                    });
            }
        }


        /// <summary>
        ///     Gets or sets the file system context menu providers.
        /// </summary>
        /// <value>
        ///     The file system context menu providers.
        /// </value>
        internal IFileSystemContextMenuProvider[] FileSystemContextMenuProviders { get; set; }

        /// <summary>
        ///     Gets or sets the everything service.
        /// </summary>
        /// <value>
        ///     The everything service.
        /// </value>
        internal EverythingService EverythingService { get; set; }

        /// <summary>
        ///     Gets or sets the search text.
        /// </summary>
        /// <value>
        ///     The search text.
        /// </value>
        public string SearchText { get; set; }

        /// <summary>
        ///     Gets or sets the everything results.
        /// </summary>
        /// <value>
        ///     The everything results.
        /// </value>
        public ObservableCollection<EverythingResultViewModel> EverythingResults { get; set; } =
            new ObservableCollection<EverythingResultViewModel>();

        /// <summary>
        ///     Gets or sets the context menu items.
        /// </summary>
        /// <value>
        ///     The context menu items.
        /// </value>
        public ObservableCollection<FrameworkElement> ContextMenuItems { get; set; } =
            new ObservableCollection<FrameworkElement>();

        /// <summary>
        ///     Gets the cancellation token source.
        /// </summary>
        /// <value>
        ///     The cancellation token source.
        /// </value>
        internal CancellationTokenSource CancellationTokenSource { get; private set; }

        /// <summary>
        ///     Gets or sets the selected items.
        /// </summary>
        /// <value>
        ///     The selected items.
        /// </value>
        internal List<EverythingResultViewModel> SelectedItems { get; set; }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Handles the preview key up.
        /// </summary>
        /// <param name="keyEventArgs">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        internal void HandlePreviewKeyUp(KeyEventArgs keyEventArgs)
        {
            // todo: use new style
            if (keyEventArgs.Key == Key.Enter || keyEventArgs.Key == Key.Return)
                Submit();
        }

        private ILog Log { get; set; } = LogManager.GetLogger<EverythingLauncherViewModel>();

        private IDisposable _previewMouseDoubleClickSubscription;
        public IObservable<(EverythingResultViewModel, MouseButtonEventArgs)> PreviewMouseDoubleClickObs
        {
            get => _previewMouseDoubleClickObs;
            set
            {
                _previewMouseDoubleClickSubscription?.Dispose();
                _previewMouseDoubleClickObs = value;
                _previewMouseDoubleClickSubscription = value.Subscribe(tuple =>
                {
                    Log.Trace($"Starting {tuple.Item1.FullName}");
                    Process.Start(tuple.Item1.FullName);
                });
            }
        }

        /// <summary>
        ///     Launches the currently selected item using whatever the shell deems appropriate
        /// </summary>
        internal void Submit()
        {
            foreach (var everythingResultViewModel in SelectedItems)
            {
                Log.Trace($"Starting {everythingResultViewModel.FullName}");
                Process.Start(everythingResultViewModel.FullName);
            }
                
        }
    }
}