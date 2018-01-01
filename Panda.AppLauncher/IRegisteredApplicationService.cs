using System;

namespace Panda.AppLauncher
{
    /// <summary>
    ///     A service that is capable of managing the RegisteredApplication domain
    /// </summary>
    public interface IRegisteredApplicationService
    {
        /// <summary>
        ///     Gets an observable that will deliver newly registered applications
        /// </summary>
        /// <value>
        ///     The application registered observable.
        /// </value>
        IObservable<RegisteredApplication> ApplicationRegisteredObservable { get; }

        /// <summary>
        ///     Gets an observable that will delivery notification of removed applications
        /// </summary>
        /// <value>
        ///     The application unregistered observable.
        /// </value>
        IObservable<RegisteredApplication> ApplicationUnregisteredObservable { get; }

        /// <summary>
        ///     Registers a new application
        /// </summary>
        /// <param name="registeredApplication">The registered application to register.</param>
        void Add(RegisteredApplication registeredApplication);

        /// <summary>
        ///     Gets an observable that will deliver all registered applications when called
        /// </summary>
        /// <returns>An observable that will deliver all currently registered applications</returns>
        IObservable<RegisteredApplication> Get();

        /// <summary>
        ///     Unregisters an application
        /// </summary>
        /// <param name="registeredApplication">The registered application to remove</param>
        void Remove(RegisteredApplication registeredApplication);

        /// <summary>
        ///     Saves the current state of registered applications
        /// </summary>
        void Save();
    }
}