using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Panda.Client;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     A service that will manage the registered application domain
    /// </summary>
    /// <seealso cref="Panda.Client.IRequiresSetup" />
    /// <seealso cref="Panda.AppLauncher.IRegisteredApplicationService" />
    [Export(typeof(IRequiresSetup))]
    [Export(typeof(IRegisteredApplicationService))]
    public sealed class RegisteredApplicationService : IRequiresSetup, IRegisteredApplicationService
    {
        /// <summary>
        ///     The application registered subject
        /// </summary>
        internal Subject<RegisteredApplication> ApplicationRegisteredSubject =
            new Subject<RegisteredApplication>();

        /// <summary>
        ///     The application unregistered subject
        /// </summary>
        internal Subject<RegisteredApplication> ApplicationUnregisteredSubject =
            new Subject<RegisteredApplication>();

        /// <summary>
        ///     Gets or sets the registered applications.
        /// </summary>
        /// <value>
        ///     The registered applications.
        /// </value>
        internal IList<RegisteredApplication> RegisteredApplications { get; set; } =
            new List<RegisteredApplication>();

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
        internal AppLauncherSettings Settings { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Gets an observable that will deliver newly registered applications
        /// </summary>
        /// <value>
        ///     The application registered observable.
        /// </value>
        public IObservable<RegisteredApplication> ApplicationRegisteredObservable => ApplicationRegisteredSubject;

        /// <inheritdoc />
        /// <summary>
        ///     Gets an observable that will delivery notification of removed applications
        /// </summary>
        /// <value>
        ///     The application unregistered observable.
        /// </value>
        public IObservable<RegisteredApplication> ApplicationUnregisteredObservable => ApplicationUnregisteredSubject;

        /// <inheritdoc />
        /// <summary>
        ///     Gets an observable that will deliver all registered applications when called
        /// </summary>
        /// <returns>
        ///     An observable that will deliver all currently registered applications
        /// </returns>
        public IObservable<RegisteredApplication> Get()
        {
            return RegisteredApplications.ToObservable();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Registers a new application
        /// </summary>
        /// <param name="registeredApplication">The registered application to register.</param>
        public void Add(RegisteredApplication registeredApplication)
        {
            Settings.RegisteredApplications.Add(registeredApplication);
            RegisteredApplications.Add(registeredApplication);
            ApplicationRegisteredSubject.OnNext(registeredApplication);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Unregisters an application
        /// </summary>
        /// <param name="registeredApplication">The registered application to remove</param>
        public void Remove(RegisteredApplication registeredApplication)
        {
            Settings.RegisteredApplications.Remove(registeredApplication); // todo: test
            RegisteredApplications.Remove(registeredApplication);
            ApplicationUnregisteredSubject.OnNext(registeredApplication);
        }

        /// <summary>
        /// Saves the current state of registered applications
        /// </summary>
        public void Save()
        {
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
            Settings = SettingsService.Get<AppLauncherSettings>().Single();
            foreach (var registeredApplication in Settings.RegisteredApplications)
            {
                var revivedApp = new RegisteredApplication
                {
                    DisplayName = registeredApplication.DisplayName,
                    FullPath = registeredApplication.FullPath
                };
                RegisteredApplications.Add(revivedApp);
            }
            return Task.CompletedTask;
        }
    }
}