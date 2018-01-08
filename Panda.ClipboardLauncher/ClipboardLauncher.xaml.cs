using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Panda.Client;

namespace Panda.ClipboardLauncher
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    public sealed partial class ClipboardLauncher : Launcher
    {
        public ClipboardLauncher()
        {
            InitializeComponent();
            ViewModel = new ClipboardLauncherViewModel(this);
            DataContext = ViewModel;
        }

        private ClipboardLauncherViewModel ViewModel { get; set; }
    }

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        internal static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("User32.dll")]
        internal static extern int
            SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool
            ChangeClipboardChain(IntPtr hWndRemove,
                IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int SendMessage(IntPtr hwnd, int wMsg,
            IntPtr wParam,
            IntPtr lParam);
    }

    internal sealed class ClipboardLauncherViewModel
    {
        internal ClipboardLauncher Instance { get; set; }

        public ClipboardLauncherViewModel(ClipboardLauncher instance)
        {
            Instance = instance;
            var handle = new WindowInteropHelper(Instance).EnsureHandle();
            HwndSource source = HwndSource.FromHwnd(handle);
            source?.AddHook(WndProc);
            NextClipboardViewer = (IntPtr) NativeMethods.SetClipboardViewer((int) handle);
        }

        public IntPtr NextClipboardViewer { get; set; }
        
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            Debug.WriteLine($"{hwnd} - {msg} - {wParam} - {lParam}");
            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            switch (msg)
            {
                case WM_DRAWCLIPBOARD:     
                    NativeMethods.SendMessage(NextClipboardViewer, msg, wParam,
                        lParam);
                    Debug.WriteLine($"WM_DRAWCLIPBOARD {msg}");
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
