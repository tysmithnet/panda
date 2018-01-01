using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Subjects;
using System.Windows.Controls;
using System.Windows.Input;
using Panda.Client;

namespace Panda.EverythingLauncher
{
    /// <inheritdoc />
    /// <summary>
    ///     Interaction logic for EverythingLauncher.xaml
    /// </summary>
    [Export(typeof(Launcher))]
    public sealed partial class EverythingLauncher : Launcher
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EverythingLauncher" /> class.
        /// </summary>
        public EverythingLauncher()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        /// <value>
        ///     The view model.
        /// </value>
        public EverythingLauncherViewModel ViewModel { get; set; }

        /// <summary>
        ///     Gets or sets the everything service.
        /// </summary>
        /// <value>
        ///     The everything service.
        /// </value>
        [Import]
        public EverythingService EverythingService { get; set; }

        /// <summary>
        ///     Gets or sets the file system context menu providers.
        /// </summary>
        /// <value>
        ///     The file system context menu providers.
        /// </value>
        [ImportMany]
        public IFileSystemContextMenuProvider[] FileSystemContextMenuProviders { get; set; }

        /// <summary>
        ///     Gets the text changed observable.
        /// </summary>
        /// <value>
        ///     The text changed observable.
        /// </value>
        private BehaviorSubject<string> TextChangedObservable { get; } = new BehaviorSubject<string>("");

        /// <summary>
        ///     Gets the selected items changed observable.
        /// </summary>
        /// <value>
        ///     The selected items changed observable.
        /// </value>
        private BehaviorSubject<IEnumerable<EverythingResultViewModel>> SelectedItemsChangedObservable { get; } =
            new BehaviorSubject<IEnumerable<EverythingResultViewModel>>(null);

        /// <summary>
        ///     Gets the preview mouse right button down observable.
        /// </summary>
        /// <value>
        ///     The preview mouse right button down observable.
        /// </value>
        private BehaviorSubject<MouseButtonEventArgs> PreviewMouseRightButtonDownObservable { get; } =
            new BehaviorSubject<MouseButtonEventArgs>(null);

        /// <summary>
        ///     Handles the OnTextChanged event of the TextBoxBase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextChangedObservable.OnNext(SearchText.Text);
        }

        /// <summary>
        ///     Handles the OnActivated event of the EverythingLauncher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void EverythingLauncher_OnActivated(object sender, EventArgs e)
        {
            SearchText.Focus();
            ViewModel = new EverythingLauncherViewModel(EverythingService, FileSystemContextMenuProviders,
                TextChangedObservable, SelectedItemsChangedObservable, PreviewMouseRightButtonDownObservable);
            DataContext = ViewModel;
        }

        /// <summary>
        ///     Handles the OnSelectionChanged event of the Selector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var results = ResultsListBox.SelectedItems.Cast<EverythingResultViewModel>().ToList();
            SelectedItemsChangedObservable.OnNext(results);
        }

        /// <summary>
        ///     Handles the OnPreviewMouseRightButtonDown event of the ResultsListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void ResultsListBox_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseRightButtonDownObservable.OnNext(e);
        }

        /// <summary>
        ///     Handles the OnPreviewKeyUp event of the ResultsListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        private void ResultsListBox_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            ViewModel.HandlePreviewKeyUp(e);
        }
    }
}