using System;

namespace Panda.Client
{
    /// <summary>
    ///     Domain event that is raised when a component deletes a file or detects a deleted file
    ///     Consumers should be wary of duplicate events as it is conveivable multiple separate watchers
    ///     could detect the same file deletion
    /// </summary>
    /// <seealso cref="Panda.Client.IDomainEvent" />
    public sealed class FileDeletedEvent : IDomainEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FileDeletedEvent" /> class.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <exception cref="ArgumentNullException">fullName</exception>
        public FileDeletedEvent(string fullName)
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        }

        /// <summary>
        ///     Gets the full name of the deleted file
        /// </summary>
        /// <example>"C:/temp/delete/this/file.txt"</example>
        /// <value>
        ///     The full name.
        /// </value>
        public string FullName { get; }
    }
}