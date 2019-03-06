using Sawmill.Models;

namespace Sawmill.Statistics.Collectors.Abstractions
{
    public interface IStatisticsCollector
    {
        string Name { get; }
        string Value { get; }

        void Process(LogEntry logEntry);
    }
}
