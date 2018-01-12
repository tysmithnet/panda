using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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
        ///     The preview mouse double click obs
        /// </summary>
        private IObservable<(EverythingResultViewModel, MouseButtonEventArgs)> _previewMouseDoubleClickObs;

        /// <summary>
        ///     The preview mouse double click subscription
        /// </summary>
        private IDisposable _previewMouseDoubleClickSubscription;

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

        /// <summary>
        ///     Initializes a new instance of the <see cref="EverythingLauncherViewModel" /> class.
        /// </summary>
        /// <param name="uiScheduler"></param>
        /// <param name="everythingService">The everything service.</param>
        /// <param name="keyboardMouseService"></param>
        /// <param name="fileSystemContextMenuProviders">The file system context menu providers.</param>
        /// <param name="eventHub"></param>
        public EverythingLauncherViewModel(IScheduler uiScheduler, IEverythingService everythingService, IKeyboardMouseService keyboardMouseService, IFileSystemContextMenuProvider[] fileSystemContextMenuProviders, IEventHub eventHub)
        {
            UiScheduler = uiScheduler;
            EverythingService = everythingService;
            FileSystemContextMenuProviders = fileSystemContextMenuProviders;
            KeyboardMouseService = keyboardMouseService;
            EventHub = eventHub;
            EventHub.Get<FileDeletedEvent>()
                .SubscribeOn(TaskPoolScheduler.Default)
                .ObserveOn(UiScheduler)
                .Subscribe(@event =>
            {
                var toRemove = EverythingResults.Where(model => model.FullName == @event.FullName);
                foreach (var everythingResultViewModel in toRemove)
                    Application.Current.Dispatcher.Invoke(
                        () => { EverythingResults.Remove(everythingResultViewModel); });
            });
        }

        public IScheduler UiScheduler { get; set; }


        /// <summary>
        ///     Gets or sets the refresh data grid action.
        /// </summary>
        /// <value>
        ///     The refresh data grid action.
        /// </value>
        internal Action RefreshDataGridAction { get; set; }

        /// <summary>
        ///     Gets or sets the preview mouse right button down obs.
        /// </summary>
        /// <value>
        ///     The preview mouse right button down obs.
        /// </value>
        internal IObservable<MouseButtonEventArgs> PreviewMouseRightButtonDownObs
        {
            get => _previewMouseRightButtonDownObs;
            set
            {
                _previewMouseRightButtonDownSubscription?.Dispose();
                _previewMouseRightButtonDownObs = value;
                _previewMouseRightButtonDownSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
                    .Where(args => args != null)
                    .WithLatestFrom(SelectedItemsChangedObs,
                        (args, models) => (args, models))
                    .Subscribe(tuple =>
                    {
                        if (!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt)) return;
                        var shellContextMenu = new ShellContextMenu();
                        var point = Mouse.GetPosition(null);
                        var fileInfos = tuple.Item2.Select(vm => new FileInfo(vm.FullName)).ToList();
                        var directories = fileInfos.Where(info =>
                            {
                                try
                                {
                                    return info.Attributes.HasFlag(FileAttributes.Directory);
                                }
                                catch (Exception e)
                                {
                                    Log.Error($"Couldn't get attributes for {info.FullName} - {e.Message}");
                                    return false;
                                }
                            })
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
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the selected items changed obs.
        /// </summary>
        /// <value>
        ///     The selected items changed obs.
        /// </value>
        internal IObservable<IEnumerable<EverythingResultViewModel>> SelectedItemsChangedObs
        {
            get => _selectedItemsChangedObs;
            set
            {
                _selectedItemsChangedSubscription?.Dispose();
                _selectedItemsChangedObs = value;
                _selectedItemsChangedSubscription = value
                    .Throttle(TimeSpan.FromMilliseconds(333))
                    .Where(model => model != null)
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
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
        internal IObservable<string> TextChangedObs
        {
            get => _textChangedObs;
            set
            {
                _textChangedSubscription?.Dispose();
                _textChangedObs = value;
                _textChangedSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
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
                            .SubscribeOn(TaskPoolScheduler.Default)
                            .ObserveOn(UiScheduler)
                            .Subscribe(async result =>
                            {
                                var resultVm = new EverythingResultViewModel(result.FullPath);
                                await Application.Current.Dispatcher.InvokeAsync(async () =>
                                {
                                    EverythingResults.Add(resultVm);
                                    await resultVm.LoadIcon();
                                });
                            }, exception =>
                            {
                                // todo: log
                            }, () => { RefreshDataGridAction?.Invoke(); });
                    });
            }
        }


        /// <summary>
        ///     Gets or sets the file system context menu providers.
        /// </summary>
        /// <value>
        ///     The file system context menu providers.
        /// </value>
        private IFileSystemContextMenuProvider[] FileSystemContextMenuProviders { get; }

        /// <summary>
        ///     Gets or sets the keyboard mouse service.
        /// </summary>
        /// <value>
        ///     The keyboard mouse service.
        /// </value>
        private IKeyboardMouseService KeyboardMouseService { get; }

        /// <summary>
        ///     Gets the event hub.
        /// </summary>
        /// <value>
        ///     The event hub.
        /// </value>
        private IEventHub EventHub { get; }

        /// <summary>
        ///     Gets or sets the everything service.
        /// </summary>
        /// <value>
        ///     The everything service.
        /// </value>
        private IEverythingService EverythingService { get; }

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
        private CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        ///     Gets or sets the selected items.
        /// </summary>
        /// <value>
        ///     The selected items.
        /// </value>
        private List<EverythingResultViewModel> SelectedItems { get; set; }

        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>
        ///     The log.
        /// </value>
        private ILog Log { get; } = LogManager.GetLogger<EverythingLauncherViewModel>();

        /// <summary>
        ///     Gets or sets the preview mouse double click obs.
        /// </summary>
        /// <value>
        ///     The preview mouse double click obs.
        /// </value>
        internal IObservable<(EverythingResultViewModel, MouseButtonEventArgs)> PreviewMouseDoubleClickObs
        {
            get => _previewMouseDoubleClickObs;
            set
            {
                _previewMouseDoubleClickSubscription?.Dispose();
                _previewMouseDoubleClickObs = value;
                _previewMouseDoubleClickSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .Subscribe(tuple =>
                {
                    Log.Trace($"Starting {tuple.Item1.FullName}");
                    Process.Start(tuple.Item1.FullName);
                });
            }
        }

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
    }
}