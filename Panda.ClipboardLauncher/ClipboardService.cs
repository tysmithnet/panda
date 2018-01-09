using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Panda.Client;

namespace Panda.ClipboardLauncher
{
    public class ClipboardService : IRequiresSetup
    {
        public Task Setup(CancellationToken cancellationToken)
        {
            
            return Task.CompletedTask;
        }
    }
}
