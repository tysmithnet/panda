using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using Panda.Client;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    ///     View model for clipboard launcher
    /// </summary>
    internal sealed class ClipboardLauncherViewModel
    {
        /// <summary>
        /// Settings service
        /// </summary>
        private ISettingsService _settingsService;

        /// <summary>
        /// Settings
        /// </summary>
        private ClipboardLauncherSettings _settings;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClipboardLauncherViewModel" /> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="settingsService"></param>
        public ClipboardLauncherViewModel(ClipboardLauncher instance, ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _settings = settingsService.Get<ClipboardLauncherSettings>().Single();
            _instance = instance;
            var handle = new WindowInteropHelper(_instance).EnsureHandle();
            var source = HwndSource.FromHwnd(handle);
            source?.AddHook(WndProc);
            _nextClipboardViewer = (IntPtr) NativeMethods.SetClipboardViewer((int) handle);
        }

        /// <summary>
        ///     Gets or sets the instance.
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        private readonly ClipboardLauncher _instance;

        /// <summary>
        ///     Gets or sets the next clipboard viewer.
        /// </summary>
        /// <value>
        ///     The next clipboard viewer.
        /// </value>
        private IntPtr _nextClipboardViewer;

        /// <summary>
        ///     Gets or sets the clipboard history.
        /// </summary>
        /// <value>
        ///     The clipboard history.
        /// </value>
        public ObservableCollection<string> ClipboardHistory { get; set; } = new ObservableCollection<string>();

        /// <summary>
        ///     Callback for the message loop
        /// </summary>
        /// <param name="hwnd">The HWND.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="wParam">The w parameter.</param>
        /// <param name="lParam">The l parameter.</param>
        /// <param name="handled">if set to <c>true</c> [handled].</param>
        /// <returns></returns>
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
                    var text = Clipboard.GetText();
                    if (ClipboardHistory.Count >= _settings.ClipboardHistorySize) // todo: make setting
                        ClipboardHistory.RemoveAt(0);
                    ClipboardHistory.Add(text);
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
    }
}