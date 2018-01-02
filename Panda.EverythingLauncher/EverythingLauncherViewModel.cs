using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
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
        ///     Initializes a new instance of the <see cref="EverythingLauncherViewModel" /> class.
        /// </summary>
        /// <param name="everythingService">The everything service.</param>
        /// <param name="fileSystemContextMenuProviders">The file system context menu providers.</param>
        /// <param name="textChangedObservable">The text changed observable.</param>
        /// <param name="selectedItemsChangedObservable">The selected items changed observable.</param>
        /// <param name="previewMouseRightButtonDownObservable">The preview mouse right button down observable.</param>
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

        /// <summary>
        ///     Gets or sets the selected items changed subscription.
        /// </summary>
        /// <value>
        ///     The selected items changed subscription.
        /// </value>
        internal IDisposable SelectedItemsChangedSubscription { get; set; }

        /// <summary>
        ///     Gets or sets the text changed subscription.
        /// </summary>
        /// <value>
        ///     The text changed subscription.
        /// </value>
        internal IDisposable TextChangedSubscription { get; set; }

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
        ///     Gets or sets the subscription.
        /// </summary>
        /// <value>
        ///     The subscription.
        /// </value>
        internal IDisposable Subscription { get; set; }

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
        ///     Handles the selected results changed.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        internal void HandleSelectedResultsChanged(IEnumerable<EverythingResultViewModel> selectedItems)
        {
            ContextMenuItems = new ObservableCollection<FrameworkElement>();
            SelectedItems = selectedItems.ToList();
            var fileInfos = SelectedItems.Select(s => new FileInfo(s.FullName));
            var providers = FileSystemContextMenuProviders.Where(f => f.CanHandle(fileInfos));
            var menuItems = providers.SelectMany(p => p.GetContextMenuItems(fileInfos));
            foreach (var frameworkElement in menuItems)
                ContextMenuItems.Add(frameworkElement);
        }

        /// <summary>
        ///     Handles the search text changed.
        /// </summary>
        /// <param name="newText">The new text.</param>
        internal void HandleSearchTextChanged(string newText)
        {
            CancellationTokenSource?.Cancel();
            CancellationTokenSource = new CancellationTokenSource();
            Subscription?.Dispose();
            EverythingResults.Clear();
            Subscription = EverythingService
                .Search(newText, CancellationTokenSource.Token)
                .ForEachAsync(
                    async result =>
                    {
                        var resultVm = new EverythingResultViewModel(result.FullPath);
                        Application.Current.Dispatcher.Invoke(() => { EverythingResults.Add(resultVm); });
                        await resultVm.LoadIcon();
                    }, CancellationTokenSource.Token);
        }

        /// <summary>
        ///     Handles the preview mouse right button down.
        /// </summary>
        /// <param name="mouseButtonEventArgs">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        /// <param name="selectedEverythingResultViewModels">The selected everything result view models.</param>
        internal void HandlePreviewMouseRightButtonDown(MouseButtonEventArgs mouseButtonEventArgs,
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
                if (!files.Any())
                    shellContextMenu.ShowContextMenu(directoryInfos, pointInfo);
                else if (!directories.Any())
                    shellContextMenu.ShowContextMenu(files, pointInfo);
                else
                    shellContextMenu.ShowContextMenu(files, directoryInfos, pointInfo);
            }
        }

        /// <summary>
        ///     Handles the preview key up.
        /// </summary>
        /// <param name="keyEventArgs">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        internal void HandlePreviewKeyUp(KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Enter || keyEventArgs.Key == Key.Return)
                Submit();
        }

        /// <summary>
        ///     Launches the currently selected item using whatever the shell deems appropriate
        /// </summary>
        internal void Submit()
        {
            foreach (var everythingResultViewModel in SelectedItems)
                Process.Start(everythingResultViewModel.FullName);
        }
    }
}