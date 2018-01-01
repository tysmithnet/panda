using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
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
        /// <summary>
        ///     Initializes a new instance of the <see cref="AppLauncher" /> class.
        /// </summary>
        public AppLauncher()
        {
            InitializeComponent();
        }

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
                RegisteredApplicationContextMenuProviders);
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
    }
}