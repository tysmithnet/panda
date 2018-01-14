using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Panda.Client;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    ///     Class ClipboardService. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Panda.Client.IRequiresSetup" />
    /// <seealso cref="Panda.ClipboardLauncher.IClipboardService" />
    [Export(typeof(IRequiresSetup))]
    [Export(typeof(IClipboardService))]
    internal sealed class ClipboardService : IRequiresSetup, IClipboardService
    {
        /// <summary>
        ///     The clipboard item subject
        /// </summary>
        private readonly Subject<ClipboardItem> _clipboardItemSubject = new Subject<ClipboardItem>();

        /// <summary>
        ///     The next clipboard viewer
        /// </summary>
        private IntPtr _nextClipboardViewer;

        /// <summary>
        ///     The text clipboard items
        /// </summary>
        private readonly List<TextClipboardItem> _textClipboardItems = new List<TextClipboardItem>();

        /// <summary>
        ///     Gets or sets the system information service.
        /// </summary>
        /// <value>The system information service.</value>
        [Import]
        public ISystemInformationService SystemInformationService { get; set; }

        /// <summary>
        ///     Gets this instance.
        /// </summary>
        /// <typeparam name="TItem">The type of the t item.</typeparam>
        /// <returns>IObservable&lt;TItem&gt;.</returns>
        public IObservable<TItem> Get<TItem>() where TItem : ClipboardItem
        {
            return _clipboardItemSubject.OfType<TItem>();
        }

        /// <summary>
        ///     Searches the specified s.
        /// </summary>
        /// <typeparam name="TItem">The type of the t item.</typeparam>
        /// <param name="s">The s.</param>
        /// <returns>IEnumerable&lt;TItem&gt;.</returns>
        public IEnumerable<TItem> Search<TItem>(string s) where TItem : ClipboardItem
        {
            return _textClipboardItems.Where(item => item.Content.ToLower().Contains(s)).OfType<TItem>();
        }

        /// <summary>
        ///     Sets the clipboard.
        /// </summary>
        /// <param name="item">The item.</param>
        public void SetClipboard(ClipboardItem item)
        {
            item.SetClipboard();
        }

        /// <summary>
        ///     Performs any setup logic required
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task, that when complete, will indicate that the setup is complete</returns>
        public Task Setup(CancellationToken cancellationToken)
        {
            var handle = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
            var source = HwndSource.FromHwnd(handle);
            source?.AddHook(WndProc);
            _nextClipboardViewer = (IntPtr) NativeMethods.SetClipboardViewer((int) handle);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     WNDs the proc.
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
                    var newItem = new TextClipboardItem
                    {
                        ClippedDate = DateTime.Now,
                        Content = Clipboard.GetText()
                    };
                    _textClipboardItems.Add(newItem);
                    _clipboardItemSubject.OnNext(newItem);
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