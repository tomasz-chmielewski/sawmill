using System;
using System.Threading;

namespace Sawmill.Application.Abstractions
{
    public interface ISawmillApplication : IDisposable
    {
        void Run(CancellationToken cancellationToken);
    }
}
