using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Panda.Client;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     A service that will manage the launchable application domain
    /// </summary>
    /// <seealso cref="Panda.Client.IRequiresSetup" />
    /// <seealso cref="ILaunchableApplicationService" />
    [Export(typeof(IRequiresSetup))]
    [Export(typeof(ILaunchableApplicationService))]
    internal sealed class LaunchableApplicationService : IRequiresSetup, ILaunchableApplicationService
    {
        /// <summary>
        ///     The application registered subject
        /// </summary>
        internal Subject<LaunchableApplication> ApplicationRegisteredSubject =
            new Subject<LaunchableApplication>();

        /// <summary>
        ///     The application unregistered subject
        /// </summary>
        internal Subject<LaunchableApplication> ApplicationUnregisteredSubject =
            new Subject<LaunchableApplication>();

        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>The log.</value>
        private ILog Log { get; } = LogManager.GetLogger<LaunchableApplicationService>();

        /// <summary>
        ///     Gets or sets the registered applications.
        /// </summary>
        /// <value>
        ///     The registered applications.
        /// </value>
        internal IList<LaunchableApplication> RegisteredApplications
        {
            get => Settings.RegisteredApplications;
            set => Settings.RegisteredApplications = value;
        }

        /// <summary>
        ///     Gets or sets the settings service.
        /// </summary>
        /// <value>
        ///     The settings service.
        /// </value>
        [Import]
        internal ISettingsService SettingsService { get; set; }

        /// <summary>
        ///     Gets or sets the settings.
        /// </summary>
        /// <value>
        ///     The settings.
        /// </value>
        internal ApplicationLauncherSettings Settings { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Gets an observable that will deliver newly registered applications
        /// </summary>
        /// <value>
        ///     The application registered observable.
        /// </value>
        public IObservable<LaunchableApplication> ApplicationRegisteredObservable => ApplicationRegisteredSubject;

        /// <inheritdoc />
        /// <summary>
        ///     Gets an observable that will delivery notification of removed applications
        /// </summary>
        /// <value>
        ///     The application unregistered observable.
        /// </value>
        public IObservable<LaunchableApplication> ApplicationUnregisteredObservable => ApplicationUnregisteredSubject;

        /// <inheritdoc />
        /// <summary>
        ///     Gets an observable that will deliver all registered applications when called
        /// </summary>
        /// <returns>
        ///     An observable that will deliver all currently registered applications
        /// </returns>
        public IObservable<LaunchableApplication> Get()
        {
            return RegisteredApplications.ToObservable();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Registers a new application
        /// </summary>
        /// <param name="launchableApplication">The registered application to register.</param>
        public void Add(LaunchableApplication launchableApplication)
        {
            Log.Trace($"Adding launchable application: {launchableApplication.FullPath}");
            RegisteredApplications.Add(launchableApplication);
            ApplicationRegisteredSubject.OnNext(launchableApplication);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Unregisters an application
        /// </summary>
        /// <param name="launchableApplication">The registered application to remove</param>
        public void Remove(LaunchableApplication launchableApplication)
        {
            Log.Trace($"Removing launchable application: {launchableApplication.FullPath}");
            if (Settings.RegisteredApplications.Contains(launchableApplication))
                Settings.RegisteredApplications.Remove(launchableApplication);
            RegisteredApplications.Remove(launchableApplication);
            ApplicationUnregisteredSubject.OnNext(launchableApplication);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Saves the current state of registered applications
        /// </summary>
        public void Save()
        {
            Log.Trace($"Saving launchable applications");
            SettingsService.Save();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Performs any setup logic required
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A task, that when complete, will indicate that the setup is complete
        /// </returns>
        public Task Setup(CancellationToken cancellationToken)
        {
            Settings = SettingsService.Get<ApplicationLauncherSettings>().Single();
            return Task.CompletedTask;
        }
    }
}