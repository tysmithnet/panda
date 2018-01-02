using System;
using System.ComponentModel;
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
        /// <summary>
        ///     Initializes a new instance of the <see cref="LauncherSelector" /> class.
        /// </summary>
        public LauncherSelector()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the launcher service.
        /// </summary>
        /// <value>
        ///     The launcher service.
        /// </value>
        [Import]
        internal ILauncherService LauncherService { get; set; }

        /// <summary>
        ///     Gets or sets the keyboard mouse hook service.
        /// </summary>
        /// <value>
        ///     The keyboard mouse hook service.
        /// </value>
        [Import]
        internal IKeyboardMouseHookService KeyboardMouseHookService { get; set; }

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        /// <value>
        ///     The view model.
        /// </value>
        private LauncherSelectorViewModel ViewModel { get; set; }

        /// <summary>
        ///     Gets the text changed observable.
        /// </summary>
        /// <value>
        ///     The text changed observable.
        /// </value>
        private Subject<string> TextChangedSubject { get; } = new Subject<string>();

        /// <summary>
        ///     Raises the <see cref="E:Closing" /> event.
        /// </summary>
        /// <param name="e">The <see cref="CancelEventArgs" /> instance containing the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        /// <summary>
        ///     Handles the OnActivated event of the LauncherSelector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void LauncherSelector_OnActivated(object sender, EventArgs e)
        {
            SearchText.Focus();
            KeyboardMouseHookService.KeyDownObservable.Throttle(TimeSpan.FromMilliseconds(100)) // todo: setting
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(args =>
                {
                    if (args.Alt && args.KeyCode == Keys.Space)
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
            ViewModel = new LauncherSelectorViewModel(LauncherService)
            {
                TextChangedObs = TextChangedSubject,
                SelectionChangedObs = SelectionChangedObservable
            };
            DataContext = ViewModel;
        }

        private Subject<SelectionChangedEventArgs> SelectionChangedObservable { get; set; } = new Subject<SelectionChangedEventArgs>();

        /// <summary>
        ///     Handles the OnSelectionChanged event of the Selector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        internal void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChangedObservable.OnNext(e);
        }

        /// <summary>
        ///     Handles the OnTextChanged event of the TextBoxBase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        internal void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextChangedSubject.OnNext(SearchText.Text);
        }

        /// <summary>
        ///     Handles the OnPreviewKeyUp event of the LauncherSelector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs" /> instance containing the event data.</param>
        internal void LauncherSelector_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Hide();

            if (e.Key == Key.Enter || e.Key == Key.Return)
                ViewModel.StartFirst();
        }
    }
}