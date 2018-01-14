using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
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
        private readonly IList<string> _clipboardHistory = new List<string>();

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
        private IObservable<(string, MouseButtonEventArgs)> _clipboardItemMouseUpObs;

        /// <summary>
        ///     The clipboard item mouse up subscription
        /// </summary>
        private IDisposable _clipboardItemMouseUpSubscription;

        /// <summary>
        ///     Gets or sets the next clipboard viewer.
        /// </summary>
        /// <value>
        ///     The next clipboard viewer.
        /// </value>
        private IntPtr _nextClipboardViewer;

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
        public ClipboardLauncherViewModel(ClipboardLauncher instance, IScheduler uiScheduler,
            ISettingsService settingsService)
        {
            _uiScheduler = uiScheduler;
            _settingsService = settingsService;
            _settings = settingsService.Get<ClipboardLauncherSettings>().Single();
            var handle = new WindowInteropHelper(instance).EnsureHandle();
            var source = HwndSource.FromHwnd(handle);
            source?.AddHook(WndProc);
            _nextClipboardViewer = (IntPtr) NativeMethods.SetClipboardViewer((int) handle);
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
        public ObservableCollection<string> ClipboardHistory { get; set; } = new ObservableCollection<string>();

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
                    .ObserveOn(_uiScheduler)
                    .Subscribe(s =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ClipboardHistory.Clear();
                            var newItems = _clipboardHistory.Where(item => item.Contains(s));
                            foreach (var newItem in newItems)
                                ClipboardHistory.Add(newItem);
                        });
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the clipboard item selected.
        /// </summary>
        /// <value>The clipboard item selected.</value>
        public IObservable<string> ClipboardItemSelected { get; set; }

        /// <summary>
        ///     Gets or sets the clipboard item mouse up obs.
        /// </summary>
        /// <value>The clipboard item mouse up obs.</value>
        public IObservable<(string, MouseButtonEventArgs)> ClipboardItemMouseUpObs
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
                            Clipboard.SetText(item);
                    });
            }
        }

        /// <summary>
        ///     Callback for the message loop
        /// </summary>
        /// <param name="hwnd">The HWND.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="wParam">The w parameter.</param>
        /// <param name="lParam">The l parameter.</param>
        /// <param name="handled">if set to <c>true</c> [handled].</param>
        /// <returns>IntPtr.</returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            switch (msg)
            {
                case WM_DRAWCLIPBOARD:
                    NativeMethods.SendMessage(_nextClipboardViewer, msg, wParam,
                        lParam);
                    SaveClipboardItem();
                    break;

                case WM_CHANGECBCHAIN:
                    if (wParam == _nextClipboardViewer)
                        _nextClipboardViewer = lParam;
                    else
                        NativeMethods.SendMessage(_nextClipboardViewer, msg, wParam,
                            lParam);
                    break;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        ///     Saves the clipboard item.
        /// </summary>
        private void SaveClipboardItem()
        {
            var newItem = Clipboard.GetText();                                                                  
            ClipboardHistory.Remove(newItem);
            ClipboardHistory.Insert(0, newItem);
            _clipboardHistory.Remove(newItem);
            _clipboardHistory.Insert(0, newItem);
        }
    }
}