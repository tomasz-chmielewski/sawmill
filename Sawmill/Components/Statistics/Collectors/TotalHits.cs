using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;

namespace Sawmill.Components.Statistics.Collectors
{
    public class TotalHits : IStatisticsCollector
    {
        public int Count { get; private set; }

        public bool Process(ILogEntry logEntry)
        {
            this.Count++;
            return true;
        }
    }
}
