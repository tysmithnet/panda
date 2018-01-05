using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panda.Client
{
    public sealed class FileDeletedEvent : IDomainEvent
    {
        public string FullName { get; private set; }

        public FileDeletedEvent(string fullName)
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        }
    }
}
