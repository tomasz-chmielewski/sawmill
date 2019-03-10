using Sawmill.Components.Statistics.Abstractions;
using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sawmill.Components.Statistics
{
    public class StatisticsCollection : ReadOnlyCollection<IStatisticsCollector>, IStatisticsCollection
    {
        public StatisticsCollection(IList<IStatisticsCollector> list)
            : base(list)
        {
        }

        public bool Process(ILogEntry logEntry)
        {
            var isProcessed = false;
            foreach (var collector in this)
            {
                isProcessed |= collector.Process(logEntry);
            }

            return isProcessed;
        }
    }
}
