using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Panda.Client;
using Panda.CommonControls;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     Interaction logic for AppLauncher.xaml
    /// </summary>
    /// <seealso cref="T:Panda.Client.Launcher" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="T:System.Windows.Markup.IStyleConnector" />
    [Export(typeof(Launcher))]
    public sealed partial class ApplicationLauncher : Launcher
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Panda.AppLauncher.AppLauncher" /> class.
        /// </summary>
        /// <inheritdoc />
        public ApplicationLauncher()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     The add application button clicked subject
        /// </summary>
        /// <value>The add application button clicked subject.</value>
        private Subject<RoutedEventArgs> AddApplicationButtonClickedSubject { get; } = new Subject<RoutedEventArgs>();

        /// <summary>
        ///     Gets or sets the text changed subject.
        /// </summary>
        /// <value>The text changed subject.</value>
        internal Subject<string> SearchTextChangedSubject { get; set; } = new Subject<string>();

        /// <summary>
        ///     Gets or sets the preview key up subject.
        /// </summary>
        /// <value>The preview key up subject.</value>
        internal Subject<KeyEventArgs> PreviewKeyUpSubject { get; set; } = new Subject<KeyEventArgs>();

        /// <summary>
        ///     Gets or sets the preview double click subject.
        /// </summary>
        /// <value>The preview double click subject.</value>
        internal Subject<LaunchableApplicationViewModel> PreviewDoubleClickSubject { get; set; } =
            new Subject<LaunchableApplicationViewModel>();

        /// <summary>
        ///     Gets or sets the registered application service.
        /// </summary>
        /// <value>The registered application service.</value>
        [Import]
        internal ILaunchableApplicationService LaunchableApplicationService { get; set; }

        /// <summary>
        ///     Gets or sets the registered application context menu providers.
        /// </summary>
        /// <value>The registered application context menu providers.</value>
        [ImportMany]
        internal ILaunchableApplicationContextMenuProvider[] LaunchableApplicationContextMenuProviders { get; set; }

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        /// <value>The view model.</value>
        internal ApplicationLauncherViewModel ViewModel { get; set; }

        /// <summary>
        ///     Gets or sets the selected items changed subject.
        /// </summary>
        /// <value>The selected items changed subject.</value>
        internal Subject<IEnumerable<LaunchableApplicationViewModel>> SelectedItemsChangedSubject { get; set; }
            = // todo: make private field
            new Subject<IEnumerable<LaunchableApplicationViewModel>>();

        /// <summary>
        ///     Gets or sets the preview mouse double click subject.
        /// </summary>
        /// <value>The preview mouse double click subject.</value>
        internal Subject<(LaunchableApplicationViewModel, MouseButtonEventArgs)>
            PreviewMouseDoubleClickSubject { get; set; } =
            new Subject<(LaunchableApplicationViewModel, MouseButtonEventArgs)>();

        /// <summary>
        ///     Gets the mouse right button up subject.
        /// </summary>
        /// <value>The mouse right button up subject.</value>
        private Subject<(LaunchableApplicationViewModel, MouseButtonEventArgs)>
            MouseRightButtonUpSubject { get; } =
            new Subject<(LaunchableApplicationViewModel, MouseButtonEventArgs)>();

        /// <summary>
        ///     Gets or sets the keyboard mouse service.
        /// </summary>
        /// <value>The keyboard mouse service.</value>
        [Import]
        private IKeyboardMouseService KeyboardMouseService { get; set; }

        /// <summary>
        ///     Handles the OnActivated event of the AppLauncher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void AppLauncher_OnLoaded(object sender, EventArgs e)
        {
            ViewModel = new ApplicationLauncherViewModel(UiScheduler, LaunchableApplicationService,
                KeyboardMouseService,
                LaunchableApplicationContextMenuProviders)
            {
                SearchTextChangedObs = SearchTextChangedSubject,
                PreviewDoubleClickObs = PreviewDoubleClickSubject,
                PreviewKeyUpObs = PreviewKeyUpSubject,
                PreviewMouseDoubleClickObs = PreviewMouseDoubleClickSubject,
                SelectedItemsChangedObs = SelectedItemsChangedSubject,
                AddApplicationButtonClickedObs = AddApplicationButtonClickedSubject,
                MouseRightButtonUpObs = MouseRightButtonUpSubject
            };
            ViewModel.SetupSubscriptions();
            DataContext = ViewModel;
            SearchText.Focus();
        }

        /// <summary>
        ///     Handles the OnSelectionChanged event of the Selector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = RegisteredApplications.SelectedItems.Cast<LaunchableApplicationViewModel>();
            SelectedItemsChangedSubject.OnNext(selectedItems);
        }

        /// <summary>
        ///     Handles the OnTextChanged event of the TextBoxBase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void SearchTextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchTextChangedSubject.OnNext(SearchText.Text);
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
        ///     Handles the OnPreviewMouseDoubleClick event of the UIElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void LaunchableApplication_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ImageTextItem;
            var vm = item?.DataContext as LaunchableApplicationViewModel;
            PreviewMouseDoubleClickSubject.OnNext((vm, e));
        }

        /// <summary>
        ///     Adds the application button on click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        private void AddApplicationButton_OnClick(object sender, RoutedEventArgs e)
        {
            AddApplicationButtonClickedSubject.OnNext(e);
        }

        /// <summary>
        ///     Applications the launcher on activated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void ApplicationLauncher_OnActivated(object sender, EventArgs e)
        {
            if (!IsLoaded) return;
            ViewModel.OnActivated();
        }

        /// <summary>
        ///     Launchables the application on mouse right button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs" /> instance containing the event data.</param>
        private void LaunchableApplication_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ImageTextItem;
            var vm = item?.DataContext as LaunchableApplicationViewModel;
            MouseRightButtonUpSubject.OnNext((vm, e));
        }
    }
}