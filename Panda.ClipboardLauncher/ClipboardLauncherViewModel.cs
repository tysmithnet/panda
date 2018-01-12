using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Interop;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    ///     View model for clipboard launcher
    /// </summary>
    internal sealed class ClipboardLauncherViewModel
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClipboardLauncherViewModel" /> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public ClipboardLauncherViewModel(ClipboardLauncher instance)
        {
            Instance = instance;
            var handle = new WindowInteropHelper(Instance).EnsureHandle();
            var source = HwndSource.FromHwnd(handle);
            source?.AddHook(WndProc);
            NextClipboardViewer = (IntPtr) NativeMethods.SetClipboardViewer((int) handle);
        }

        /// <summary>
        ///     Gets or sets the instance.
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        internal ClipboardLauncher Instance { get; set; }

        /// <summary>
        ///     Gets or sets the next clipboard viewer.
        /// </summary>
        /// <value>
        ///     The next clipboard viewer.
        /// </value>
        private IntPtr NextClipboardViewer { get; set; }

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
                    NativeMethods.SendMessage(NextClipboardViewer, msg, wParam,
                        lParam);
                    var text = Clipboard.GetText();
                    if (ClipboardHistory.Count >= 50) // todo: make setting
                        ClipboardHistory.RemoveAt(0);
                    ClipboardHistory.Add(text);
                    break;

                case WM_CHANGECBCHAIN:
                    if (wParam == NextClipboardViewer)
                        NextClipboardViewer = lParam;
                    else
                        NativeMethods.SendMessage(NextClipboardViewer, msg, wParam,
                            lParam);
                    break;
            }
            return IntPtr.Zero;
        }
    }
}