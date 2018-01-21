using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Panda.Client
{
    /// <summary>
    ///     View model for the launcher selector
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public sealed class LauncherSelectorViewModel : INotifyPropertyChanged
    {                                                                                          
        /// <summary>
        ///     The global show launcher selector subscription
        /// </summary>
        private IDisposable _globalShowLauncherSelectorSubscription;

        /// <summary>
        ///     The keyboard mouse service
        /// </summary>
        private IKeyboardMouseService _keyboardMouseService;

        /// <summary>
        ///     The launcher selector key up obs
        /// </summary>
        private IObservable<KeyEventArgs> _launcherSelectorKeyUpObs;

        /// <summary>
        ///     The launcher selector key up subscription
        /// </summary>
        private IDisposable _launcherSelectorKeyUpSubscription;

        /// <summary>
        ///     The preview mouse up obs
        /// </summary>
        private IObservable<(LauncherViewModel, MouseButtonEventArgs)> _mouseUpObs;

        /// <summary>
        ///     The preview mouse up subscription
        /// </summary>
        private IDisposable _previewMouseUpSubscription;

        /// <summary>
        ///     The search text
        /// </summary>
        private string _searchText;

        /// <summary>
        ///     The search text box preview key up obs
        /// </summary>
        private IObservable<(string, KeyEventArgs)> _searchTextBoxPreviewKeyUpObs;

        /// <summary>
        ///     The search text box preview key up subscription
        /// </summary>
        private IDisposable _searchTextBoxPreviewKeyUpSubscription;

        /// <summary>
        ///     The selection changed obs
        /// </summary>
        private IObservable<SelectionChangedEventArgs> _selectionChangedObs;

        /// <summary>
        ///     The selection changed subscription
        /// </summary>
        private IDisposable _selectionChangedSubscription;

        /// <summary>
        ///     The text changed obs
        /// </summary>
        private IObservable<string> _textChangedObs;

        /// <summary>
        ///     The text changed subscription
        /// </summary>
        private IDisposable _textChangedSubscription;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LauncherSelectorViewModel" /> class.
        /// </summary>
        /// <param name="uiScheduler">The UI scheduler.</param>
        /// <param name="launcherService">The launcher service.</param>
        /// <param name="keyboardMouseService">The keyboard mouse service.</param>
        internal LauncherSelectorViewModel(IScheduler uiScheduler, ILauncherService launcherService,
            IKeyboardMouseService keyboardMouseService)
        {
            UiScheduler = uiScheduler;
            LauncherService = launcherService;
            KeyboardMouseService = keyboardMouseService;
            ViewModels = LauncherService.Get().Select(l => new LauncherViewModel
            {
                Name = l.GetType().Name,
                Instance = l
            });
            LauncherViewModels = new ObservableCollection<LauncherViewModel>(ViewModels);
        }

        private LauncherViewModel _lastActiveViewModel;

        /// <summary>
        ///     Gets or sets the keyboard mouse service.
        /// </summary>
        /// <value>The keyboard mouse service.</value>
        public IKeyboardMouseService KeyboardMouseService
        {
            get => _keyboardMouseService;
            set
            {
                _globalShowLauncherSelectorSubscription?.Dispose();
                _keyboardMouseService = value;
                _globalShowLauncherSelectorSubscription = value
                    .KeyDownObservable
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
                    .Subscribe(args =>
                    {                               
                        if (args.Control && args.Alt && args.KeyCode == Keys.Oem3)
                        {
                            foreach (var launcherViewModel in ViewModels.Where(vm => vm.Instance.IsLoaded))
                            {
                                ActivateLauncher(launcherViewModel);
                                args.Handled = true;
                            }
                            return;
                        }

                        if (args.Control && args.Shift && args.KeyCode == Keys.Oem3)
                        {
                            if (_lastActiveViewModel != null)
                            {
                                ActivateLauncher(_lastActiveViewModel);
                                args.Handled = true;
                            }
                            return;
                        }
                         
                        if (args.Control && args.KeyCode == Keys.Oem3) // `
                        {
                            ShowAction?.Invoke();
                            args.Handled = true;
                        }
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the UI scheduler.
        /// </summary>
        /// <value>The UI scheduler.</value>
        public IScheduler UiScheduler { get; set; }

        /// <summary>
        ///     Gets or sets the text changed obs.
        /// </summary>
        /// <value>The text changed obs.</value>
        internal IObservable<string> TextChangedObs
        {
            get => _textChangedObs;
            set
            {
                _textChangedSubscription?.Dispose();
                _textChangedObs = value;
                _textChangedSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
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
                        {
                            try
                            {
                                return Regex.IsMatch(vm.Name, filter, RegexOptions.IgnoreCase); // todo: allow for basic substring matching, and fuzzy
                            }
                            catch (Exception)
                            {
                                return false;
                            }
                        }))
                            LauncherViewModels.Add(launcherViewModel);
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the launcher service.
        /// </summary>
        /// <value>The launcher service.</value>
        private ILauncherService LauncherService { get; }

        /// <summary>
        ///     Gets or sets the view models.
        /// </summary>
        /// <value>The view models.</value>
        private IEnumerable<LauncherViewModel> ViewModels { get; }

        /// <summary>
        ///     Gets or sets the launcher view models.
        /// </summary>
        /// <value>The launcher view models.</value>
        public ObservableCollection<LauncherViewModel> LauncherViewModels { get; set; }

        /// <summary>
        ///     Gets or sets the search text.
        /// </summary>
        /// <value>The search text.</value>
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the selection changed obs.
        /// </summary>
        /// <value>The selection changed obs.</value>
        public IObservable<SelectionChangedEventArgs> SelectionChangedObs
        {
            get => _selectionChangedObs;
            set
            {
                _selectionChangedSubscription?.Dispose();
                _selectionChangedObs = value;
                _selectionChangedSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
                    .Subscribe(e =>
                    {
                        var added = e.AddedItems.Cast<LauncherViewModel>();
                        var launcherViewModels = added as LauncherViewModel[] ?? added.ToArray();
                        if (launcherViewModels.Any())
                        {
                            var first = launcherViewModels.First();
                            first.Instance.Show();
                            SearchText = "";
                        }
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the preview mouse up obs.
        /// </summary>
        /// <value>The preview mouse up obs.</value>
        internal IObservable<(LauncherViewModel, MouseButtonEventArgs)> MouseUpObs
        {
            get => _mouseUpObs;
            set
            {
                _previewMouseUpSubscription?.Dispose();
                _mouseUpObs = value;
                _previewMouseUpSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
                    .Subscribe(tuple => ActivateLauncher(tuple.Item1));
            }
        }

        /// <summary>
        ///     Gets or sets the search text box preview key up obs.
        /// </summary>
        /// <value>The search text box preview key up obs.</value>
        internal IObservable<(string, KeyEventArgs)> SearchTextBoxPreviewKeyUpObs
        {
            get => _searchTextBoxPreviewKeyUpObs;
            set
            {
                _searchTextBoxPreviewKeyUpSubscription?.Dispose();
                _searchTextBoxPreviewKeyUpObs = value;
                _searchTextBoxPreviewKeyUpSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
                    .Subscribe(tuple =>
                    {
                        var currentText = tuple.Item1;
                        var args = tuple.Item2;

                        if (new[] {Key.Enter, Key.Return}.Contains(args.Key))
                        {
                            StartFirst();
                            args.Handled = true;
                        }
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the hide action.
        /// </summary>
        /// <value>The hide action.</value>
        internal Action HideAction { get; set; }

        /// <summary>
        ///     Gets or sets the launcher selector key up obs.
        /// </summary>
        /// <value>The launcher selector key up obs.</value>
        internal IObservable<KeyEventArgs> LauncherSelectorKeyUpObs
        {
            get => _launcherSelectorKeyUpObs;
            set
            {
                _launcherSelectorKeyUpSubscription?.Dispose();
                _launcherSelectorKeyUpObs = value;
                _launcherSelectorKeyUpSubscription = value
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(UiScheduler)
                    .Subscribe(args =>
                    {
                        if (args.Key == Key.Escape)
                        {
                            HideAction?.Invoke();
                            args.Handled = true;
                        }
                    });
            }
        }

        /// <summary>
        ///     Gets or sets the show action.
        /// </summary>
        /// <value>The show action.</value>
        public Action ShowAction { get; set; }

        /// <summary>
        ///     Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Activates the launcher.
        /// </summary>
        /// <param name="vm">The vm.</param>
        private void ActivateLauncher(LauncherViewModel vm)
        {   
            var instance = vm.Instance;
            instance.Show();
            instance.WindowState = WindowState.Normal;
            instance.BringIntoView();
            SearchText = "";
            _lastActiveViewModel = vm;
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
        ///     Submits this instance.
        /// </summary>
        private void StartFirst()
        {
            var first = LauncherViewModels.FirstOrDefault();
            if (first == null) return;
            ActivateLauncher(first);
        }
    }
}