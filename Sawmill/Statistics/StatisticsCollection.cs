using Sawmill.Models;
using Sawmill.Statistics.Collectors.Abstractions;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sawmill.Statistics
{
    public class StatisticsCollection : ReadOnlyCollection<IStatisticsCollector>
    {
        public StatisticsCollection(IList<IStatisticsCollector> list)
            : base(list)
        {
        }

        public bool Process(LogEntry logEntry)
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
