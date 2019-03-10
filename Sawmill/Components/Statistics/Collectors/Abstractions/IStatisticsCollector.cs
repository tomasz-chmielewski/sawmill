using Sawmill.Data.Models.Abstractions;

namespace Sawmill.Components.Statistics.Collectors.Abstractions
{
    public interface IStatisticsCollector
    {
        string Name { get; }
        string Value { get; }

        bool Process(ILogEntry logEntry);
    }
}
