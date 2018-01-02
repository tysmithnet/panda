using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Subjects;
using System.Windows.Controls;
using System.Windows.Input;
using Panda.Client;

namespace Panda.AppLauncher
{
    /// <inheritdoc />
    /// <summary>
    ///     Interaction logic for AppLauncher.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    public sealed partial class AppLauncher : Launcher
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Panda.AppLauncher.AppLauncher" /> class.
        /// </summary>
        public AppLauncher()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the text changed subject.
        /// </summary>
        /// <value>
        ///     The text changed subject.
        /// </value>
        internal Subject<string> TextChangedSubject { get; set; } = new Subject<string>();

        /// <summary>
        ///     Gets or sets the preview key up subject.
        /// </summary>
        /// <value>
        ///     The preview key up subject.
        /// </value>
        internal Subject<KeyEventArgs> PreviewKeyUpSubject { get; set; } = new Subject<KeyEventArgs>();

        /// <summary>
        ///     Gets or sets the preview double click subject.
        /// </summary>
        /// <value>
        ///     The preview double click subject.
        /// </value>
        internal Subject<RegisteredApplicationViewModel> PreviewDoubleClickSubject { get; set; } =
            new Subject<RegisteredApplicationViewModel>();

        /// <summary>
        ///     Gets or sets the registered application service.
        /// </summary>
        /// <value>
        ///     The registered application service.
        /// </value>
        [Import]
        internal IRegisteredApplicationService RegisteredApplicationService { get; set; }

        /// <summary>
        ///     Gets or sets the registered application context menu providers.
        /// </summary>
        /// <value>
        ///     The registered application context menu providers.
        /// </value>
        [ImportMany]
        internal IRegisteredApplicationContextMenuProvider[] RegisteredApplicationContextMenuProviders { get; set; }

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        /// <value>
        ///     The view model.
        /// </value>
        internal AppLauncherViewModel ViewModel { get; set; }

        /// <summary>
        ///     Handles the OnActivated event of the AppLauncher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void AppLauncher_OnActivated(object sender, EventArgs e)
        {
            ViewModel = new AppLauncherViewModel(RegisteredApplicationService,
                RegisteredApplicationContextMenuProviders)
            {
                TextChangedObs = TextChangedSubject,
                PreviewDoubleClickObs = PreviewDoubleClickSubject,
                PreviewKeyUpObs = PreviewKeyUpSubject
            };
            ViewModel.Setup();
            DataContext = ViewModel;
        }

        /// <summary>
        ///     Handles the OnSelectionChanged event of the Selector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = RegisteredApplications.SelectedItems.Cast<RegisteredApplicationViewModel>();
            ViewModel.HandleSelectedItemsChanged(selectedItems);
        }

        /// <summary>
        ///     Handles the OnTextChanged event of the TextBoxBase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextChangedSubject.OnNext(SearchText.Text);
        }

        /// <summary>
        ///     Handles the OnPreviewKeyUp event of the SearchText control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        private void SearchText_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            PreviewKeyUpSubject.OnNext(e);
        }

        /// <summary>
        ///     Handles the OnPreviewMouseDoubleClick event of the Control control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void Control_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var label = sender as Label;
            var parent = label?.Parent as Grid;
            if (parent?.DataContext is RegisteredApplicationViewModel vm)
                PreviewDoubleClickSubject.OnNext(vm);
        }
    }
}