using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System.Collections.Generic;

namespace Sawmill.Components.Statistics.Abstractions
{
    public interface IStatisticsCollection : IEnumerable<IStatisticsCollector>
    {
        bool Process(ILogEntry logEntry);
    }
}
