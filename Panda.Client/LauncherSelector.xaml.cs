using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Panda.Client
{
    /// <summary>
    ///     Interaction logic for LauncherSelector.xaml
    /// </summary>
    [Export(typeof(LauncherSelector))]
    public sealed partial class LauncherSelector : Window
    {
        public LauncherSelector()
        {
            InitializeComponent();
        }

        [Import]
        internal ILauncherService LauncherService { get; set; }

        [Import]
        internal IKeyboardMouseHookService KeyboardMouseHookService { get; set; }

        private LauncherSelectorViewModel ViewModel { get; set; }
        private BehaviorSubject<string> TextChangedObservable { get; } = new BehaviorSubject<string>("");

        private void LauncherSelector_OnActivated(object sender, EventArgs e)
        {
            SearchText.Focus();
            KeyboardMouseHookService.KeyDownObservable.Throttle(TimeSpan.FromMilliseconds(100)) // todo: setting
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(args =>
                {
                    if (args.Alt && args.KeyCode.HasFlag(Keys.Space))
                    {
                        WindowState = WindowState.Normal;
                        //Topmost = true;
                        Show();
                        Activate();
                        Focus();
                        SearchText.Focus();
                        args.Handled = true;
                    }
                });
            ViewModel = new LauncherSelectorViewModel(LauncherService,
                TextChangedObservable);
            DataContext = ViewModel;
        }

        internal void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.Handle(e); // todo: rename
        }

        internal void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextChangedObservable.OnNext(SearchText.Text);
        }

        internal void LauncherSelector_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Hide();

            if (e.Key == Key.Enter || e.Key == Key.Return)
                ViewModel.Submit();
        }
    }
}