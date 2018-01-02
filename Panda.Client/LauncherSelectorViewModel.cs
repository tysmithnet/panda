using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
        private IObservable<string> _textChangedObs;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LauncherSelectorViewModel" /> class.
        /// </summary>
        /// <param name="launcherService">The launcher service.</param>
        public LauncherSelectorViewModel(ILauncherService launcherService)
        {
            LauncherService = launcherService;
            ViewModels = LauncherService.Get().Select(l => new LauncherViewModel
            {
                Name = l.GetType().FullName,
                Instance = l
            });
            LauncherViewModels = new ObservableCollection<LauncherViewModel>(ViewModels);
        }

        private IDisposable _textChangedSubscription;
        private IObservable<SelectionChangedEventArgs> _selectionChangedObs;

        public IObservable<string> TextChangedObs
        {
            get => _textChangedObs;
            set
            {
                _textChangedSubscription?.Dispose();
                _textChangedObs = value; 
                _textChangedSubscription = value
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(filter =>
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
                    });
            }
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

        private IDisposable _selectionChangedSubscription;
        public IObservable<SelectionChangedEventArgs> SelectionChangedObs
        {
            get => _selectionChangedObs;
            set
            {
                _selectionChangedSubscription?.Dispose();
                _selectionChangedObs = value;
                _selectionChangedSubscription = value.Subscribe(e =>
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
                });                 
            }
        }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Submits this instance.
        /// </summary>
        public void StartFirst()
        {
            var first = LauncherViewModels.FirstOrDefault();
            if (first == null) return;
            Active?.Hide();
            Active = first.Instance;
            Active.Show();
            // todo: allow multiple active windows
        }
    }
}