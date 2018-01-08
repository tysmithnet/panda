using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Subjects;
using System.Windows;
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
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Panda.EverythingLauncher.EverythingLauncher" /> class.
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
        public IEverythingService EverythingService { get; set; }

        /// <summary>
        ///     Gets or sets the file system context menu providers.
        /// </summary>
        /// <value>
        ///     The file system context menu providers.
        /// </value>
        [ImportMany]
        public IFileSystemContextMenuProvider[] FileSystemContextMenuProviders { get; set; }

        /// <summary>
        ///     Gets or sets the keyboard mouse service.
        /// </summary>
        /// <value>
        ///     The keyboard mouse service.
        /// </value>
        [Import]
        public IKeyboardMouseService KeyboardMouseService { get; set; }

        /// <summary>
        ///     Gets or sets the event hub.
        /// </summary>
        /// <value>
        ///     The event hub.
        /// </value>
        [Import]
        public IEventHub EventHub { get; set; }

        /// <summary>
        ///     Gets the text changed observable.
        /// </summary>
        /// <value>
        ///     The text changed observable.
        /// </value>
        private Subject<string> TextChangedObservable { get; } = new Subject<string>();

        /// <summary>
        ///     Gets the selected items changed observable.
        /// </summary>
        /// <value>
        ///     The selected items changed observable.
        /// </value>
        private Subject<IEnumerable<EverythingResultViewModel>> SelectedItemsChangedObservable { get; } =
            new Subject<IEnumerable<EverythingResultViewModel>>();

        /// <summary>
        ///     Gets the preview mouse right button down observable.
        /// </summary>
        /// <value>
        ///     The preview mouse right button down observable.
        /// </value>
        private Subject<MouseButtonEventArgs> PreviewMouseRightButtonDownObservable { get; } =
            new Subject<MouseButtonEventArgs>();

        /// <summary>
        ///     Gets or sets the preview mouse double click subject.
        /// </summary>
        /// <value>
        ///     The preview mouse double click subject.
        /// </value>
        internal Subject<(EverythingResultViewModel, MouseButtonEventArgs)> PreviewMouseDoubleClickSubject { get; set; }
            = new Subject<(EverythingResultViewModel, MouseButtonEventArgs)>();

        /// <summary>
        ///     Handles the OnTextChanged event of the TextBoxBase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextChangedObservable.OnNext(SearchText.Text);

            foreach (var dataGridColumn in ResultsDataGrid.Columns)
                dataGridColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
        }

        /// <summary>
        ///     Handles the OnActivated event of the EverythingLauncher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void EverythingLauncher_OnLoaded(object sender, EventArgs e)
        {
            SearchText.Focus();
            ViewModel = new EverythingLauncherViewModel(EverythingService, KeyboardMouseService,
                FileSystemContextMenuProviders, EventHub)
            {
                TextChangedObs = TextChangedObservable,
                SelectedItemsChangedObs = SelectedItemsChangedObservable,
                PreviewMouseRightButtonDownObs = PreviewMouseRightButtonDownObservable,
                PreviewMouseDoubleClickObs = PreviewMouseDoubleClickSubject,
                RefreshDataGridAction = () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var dataGridColumn in ResultsDataGrid.Columns)
                            dataGridColumn.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
                    });
                }
            };
            DataContext = ViewModel;
        }

        /// <summary>
        ///     Handles the OnSelectedCellsChanged event of the DataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectedCellsChangedEventArgs" /> instance containing the event data.</param>
        private void DataGrid_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var selectedVms = dataGrid?.SelectedCells.Select(info => info.Item as EverythingResultViewModel).Distinct();
            SelectedItemsChangedObservable.OnNext(selectedVms);
        }

        /// <summary>
        ///     Handles the OnPreviewMouseRightButtonDown event of the UIElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void UIElement_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseRightButtonDownObservable.OnNext(e);
        }

        /// <summary>
        ///     Handles the OnHandler event of the EventSetter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void EventSetter_OnHandler(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow dataGridRow && dataGridRow.DataContext is EverythingResultViewModel vm)
                PreviewMouseDoubleClickSubject.OnNext((vm, e));
        }
    }
}