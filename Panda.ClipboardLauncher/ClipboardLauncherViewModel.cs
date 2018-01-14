using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using Common.Logging;
using Panda.Client;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    ///     View model for clipboard launcher
    /// </summary>
    internal sealed class ClipboardLauncherViewModel
    {
        /// <summary>
        ///     The clipboard history
        /// </summary>
        private readonly IList<ClipboardItemViewModel> _clipboardHistory = new List<ClipboardItemViewModel>();

        /// <summary>
        ///     Settings
        /// </summary>
        private readonly ClipboardLauncherSettings _settings;

        /// <summary>
        ///     The UI scheduler
        /// </summary>
        private readonly IScheduler _uiScheduler;

        /// <summary>
        ///     The clipboard item mouse up obs
        /// </summary>
        private IObservable<(ClipboardItemViewModel, MouseButtonEventArgs)> _clipboardItemMouseUpObs;

        /// <summary>
        ///     The clipboard item mouse up subscription
        /// </summary>
        private IDisposable _clipboardItemMouseUpSubscription;

        /// <summary>
        ///     The clipboard service
        /// </summary>
        private readonly IClipboardService _clipboardService;

        /// <summary>
        ///     The search text changed obs
        /// </summary>
        private IObservable<string> _searchTextChangedObs;

        /// <summary>
        ///     The search text changed subscription
        /// </summary>
        private IDisposable _searchTextChangedSubscription;

        /// <summary>
        ///     Settings service
        /// </summary>
        private ISettingsService _settingsService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClipboardLauncherViewModel" /> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="uiScheduler">The UI scheduler.</param>
        /// <param name="settingsService">The settings service.</param>
        public ClipboardLauncherViewModel(IScheduler uiScheduler, IClipboardService clipboardService,
            ISettingsService settingsService)
        {
            _uiScheduler = uiScheduler;
            _clipboardService = clipboardService;
            _settingsService = settingsService;
            _settings = settingsService.Get<ClipboardLauncherSettings>().Single();

            _clipboardService.Get<TextClipboardItem>().SubscribeOn(TaskPoolScheduler.Default)
                .ObserveOn(_uiScheduler)
                .Subscribe(item =>
                {
                    var vm = new ClipboardItemViewModel(item);
                    _clipboardHistory.Remove(vm); // todo: support image, wave, file, etc
                    _clipboardHistory.Insert(0, vm);
                    ClipboardHistory.Remove(vm);
                    ClipboardHistory.Insert(0, vm);
                });

            // todo: add database service
        }

        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>The log.</value>
        private ILog Log { get; } = LogManager.GetLogger<ClipboardLauncherViewModel>();

        /// <summary>
        ///     Gets or sets the clipboard history.
        /// </summary>
        /// <value>The clipboard history.</value>
        public ObservableCollection<ClipboardItemViewModel> ClipboardHistory { get; set; } =
            new ObservableCollection<ClipboardItemViewModel>();

        /// <summary>
        ///     Gets or sets the search text changed obs.
        /// </summary>
        /// <value>The search text changed obs.</value>
        public IObservable<string> SearchTextChangedObs
        {
            get => _searchTextChangedObs;
            set
            {
                _searchTextChangedSubscription?.Dispose();
                _searchTextChangedObs = value;
                _searchTextChangedSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .Where(s => s != null)
                    .ObserveOn(_uiScheduler)
                    .Subscribe(s =>
                    {
                        Application.Current.Dispatcher.Invoke(() => ClipboardHistory.Clear());
                        if (s.Length == 0)
                            foreach (var clipboardItemViewModel in _clipboardHistory)
                                ClipboardHistory.Add(clipboardItemViewModel);
                        else
                            foreach (var textClipboardItem in _clipboardService.Search<TextClipboardItem>(s))
                                ClipboardHistory.Add(new ClipboardItemViewModel(textClipboardItem));
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the clipboard item selected.
        /// </summary>
        /// <value>The clipboard item selected.</value>
        public IObservable<ClipboardItemViewModel> ClipboardItemSelected { get; set; }

        /// <summary>
        ///     Gets or sets the clipboard item mouse up obs.
        /// </summary>
        /// <value>The clipboard item mouse up obs.</value>
        public IObservable<(ClipboardItemViewModel, MouseButtonEventArgs)> ClipboardItemMouseUpObs
        {
            get => _clipboardItemMouseUpObs;
            set
            {
                _clipboardItemMouseUpSubscription?.Dispose();
                _clipboardItemMouseUpObs = value;
                _clipboardItemMouseUpSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(_uiScheduler)
                    .Subscribe(tuple =>
                    {
                        var item = tuple.Item1;
                        var args = tuple.Item2;
                        if (item != null)
                            _clipboardService.SetClipboard(item.Instance);
                    });
            }
        }
    }
}