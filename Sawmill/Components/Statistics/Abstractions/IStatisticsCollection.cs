using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System.Collections.Generic;

namespace Sawmill.Components.Statistics.Abstractions
{
    public interface IStatisticsCollection : IReadOnlyCollection<IStatisticsCollector>
    {
        bool Process(ILogEntry logEntry);
    }
}
