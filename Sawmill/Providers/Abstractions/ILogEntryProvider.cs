using Sawmill.Models;

namespace Sawmill.Providers.Abstractions
{
    public interface ILogEntryProvider
    {
        LogEntry GetEntry();
    }
}
