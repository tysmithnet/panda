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
using Panda.CommonControls;
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
        internal IKeyboardMouseService KeyboardMouseService { get; set; }

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
        ///     Gets or sets the selection changed observable.
        /// </summary>
        /// <value>
        ///     The selection changed observable.
        /// </value>
        private Subject<SelectionChangedEventArgs> SelectionChangedSubject { get; } =
            new Subject<SelectionChangedEventArgs>();

        /// <summary>
        ///     Gets or sets the mouse up subject.
        /// </summary>
        /// <value>
        ///     The mouse up subject.
        /// </value>
        internal Subject<(LauncherViewModel, MouseButtonEventArgs)> MouseUpSubject { get; set; } =
            new Subject<(LauncherViewModel, MouseButtonEventArgs)>();

        /// <summary>
        ///     Gets or sets the search text box preview key up subject.
        /// </summary>
        /// <value>
        ///     The search text box preview key up subject.
        /// </value>
        public Subject<(string, KeyEventArgs)> SearchTextBoxPreviewKeyUpSubject { get; set; } =
            new Subject<(string, KeyEventArgs)>();

        /// <summary>
        ///     Gets or sets the launcher selected key up subject.
        /// </summary>
        /// <value>
        ///     The launcher selected key up subject.
        /// </value>
        public Subject<KeyEventArgs> LauncherSelectedKeyUpSubject { get; set; } = new Subject<KeyEventArgs>();

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
        private void LauncherSelector_OnLoaded(object sender, EventArgs e)
        {
            // todo: move to VM
            SearchText.Focus();
            KeyboardMouseService.KeyDownObservable.Throttle(TimeSpan.FromMilliseconds(100)) // todo: setting
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
                SelectionChangedObs = SelectionChangedSubject,
                MouseUpObs = MouseUpSubject,
                SearchTextBoxPreviewKeyUpObs = SearchTextBoxPreviewKeyUpSubject,
                LauncherSelectorKeyUpObs = LauncherSelectedKeyUpSubject,
                HideAction = Hide
            };
            DataContext = ViewModel;
        }

        /// <summary>
        ///     Handles the OnSelectionChanged event of the Selector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        internal void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChangedSubject.OnNext(e);
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
        ///     Handles the OnMouseUp event of the ImageTextItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void ImageTextItem_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var launcher = sender as ImageTextItem;
            var vm = launcher?.DataContext as LauncherViewModel;
            MouseUpSubject.OnNext((vm, e));
        }

        /// <summary>
        ///     Handles the OnPreviewKeyUp event of the SearchText control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        private void SearchText_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            SearchTextBoxPreviewKeyUpSubject.OnNext((SearchText.Text, e));
        }

        /// <summary>
        ///     Handles the OnKeyUp event of the LauncherSelector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        private void LauncherSelector_OnKeyUp(object sender, KeyEventArgs e)
        {
            LauncherSelectedKeyUpSubject.OnNext(e);
        }
    }
}