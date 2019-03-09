using System;

namespace Sawmill.Application.Abstractions
{
    public interface ISawmillApplicationFactory : IDisposable
    {
        ISawmillApplication Create();
    }
}
