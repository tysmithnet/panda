using System;

namespace Panda.Client
{
    public sealed class FileDeletedEvent : IDomainEvent
    {
        public FileDeletedEvent(string fullName)
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        }

        public string FullName { get; }
    }
}