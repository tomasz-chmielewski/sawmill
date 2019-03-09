using Sawmill.Models;

namespace Sawmill.Statistics.Collectors.Abstractions
{
    public interface IStatisticsCollector
    {
        string Name { get; }
        string Value { get; }

        bool Process(LogEntry logEntry);
    }
}
