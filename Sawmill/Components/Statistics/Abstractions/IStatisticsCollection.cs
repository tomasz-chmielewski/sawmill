using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Models;
using System.Collections.Generic;

namespace Sawmill.Components.Statistics.Abstractions
{
    public interface IStatisticsCollection : IReadOnlyCollection<IStatisticsCollector>
    {
        bool Process(LogEntry logEntry);
    }
}
