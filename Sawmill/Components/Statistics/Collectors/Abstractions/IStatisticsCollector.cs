using Sawmill.Data.Models.Abstractions;

namespace Sawmill.Components.Statistics.Collectors.Abstractions
{
    public interface IStatisticsCollector
    {
        bool Process(ILogEntry logEntry);
    }
}
