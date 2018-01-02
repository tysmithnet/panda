using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;

namespace Panda.Client
{
    /// <summary>
    ///     View model for the launcher selector
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public sealed class LauncherSelectorViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LauncherSelectorViewModel" /> class.
        /// </summary>
        /// <param name="launcherService">The launcher service.</param>
        /// <param name="textChangedObservable">The text changed observable.</param>
        public LauncherSelectorViewModel(ILauncherService launcherService, IObservable<string> textChangedObservable)
        {
            LauncherService = launcherService;
            ViewModels = LauncherService.Get().Select(l => new LauncherViewModel
            {
                Name = l.GetType().FullName,
                Instance = l
            });
            LauncherViewModels = new ObservableCollection<LauncherViewModel>(ViewModels);
            textChangedObservable
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(FilterApps);
        }

        /// <summary>
        ///     Gets or sets the launcher service.
        /// </summary>
        /// <value>
        ///     The launcher service.
        /// </value>
        internal ILauncherService LauncherService { get; set; }

        /// <summary>
        ///     Gets or sets the view models.
        /// </summary>
        /// <value>
        ///     The view models.
        /// </value>
        internal IEnumerable<LauncherViewModel> ViewModels { get; set; }

        /// <summary>
        ///     Gets or sets the active.
        /// </summary>
        /// <value>
        ///     The active.
        /// </value>
        internal Launcher Active { get; set; }

        /// <summary>
        ///     Gets or sets the launcher view models.
        /// </summary>
        /// <value>
        ///     The launcher view models.
        /// </value>
        public ObservableCollection<LauncherViewModel> LauncherViewModels { get; set; }

        /// <summary>
        ///     Gets or sets the search text.
        /// </summary>
        /// <value>
        ///     The search text.
        /// </value>
        public string SearchText { get; set; }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Handles the selection changing
        /// </summary>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        public void HandleSelectionChanged(SelectionChangedEventArgs e)
        {
            var added = e.AddedItems.Cast<LauncherViewModel>();
            var launcherViewModels = added as LauncherViewModel[] ?? added.ToArray();
            if (launcherViewModels.Any())
            {
                var first = launcherViewModels.First();
                Active?.Hide();
                Active = first.Instance;
                Active.Show();
            }
        }

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Filters the apps.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void FilterApps(string filter)
        {
            LauncherViewModels.Clear();

            if (string.IsNullOrEmpty(filter))
            {
                foreach (var launcherViewModel in ViewModels)
                    LauncherViewModels.Add(launcherViewModel);
                return;
            }

            foreach (var launcherViewModel in ViewModels.Where(vm =>
                Regex.IsMatch(vm.Name, filter, RegexOptions.IgnoreCase)))
                LauncherViewModels.Add(launcherViewModel);
        }

        /// <summary>
        ///     Submits this instance.
        /// </summary>
        public void Submit()
        {
            var first = LauncherViewModels.First();
            Active?.Hide();
            Active = first.Instance;
            Active.Show();
        }
    }
}