using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;

namespace Sawmill.Components.Statistics.Collectors
{
    /// <summary>
    /// Collects statistics related to total hits.
    /// </summary>
    public class TotalHits : IStatisticsCollector
    {
        /// <summary>
        /// Total number of hits.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Process the specified log entry.
        /// </summary>
        /// <param name="logEntry">Log entry to process.</param>
        /// <returns>true if the entry was processed or false otherwise.</returns>
        public bool Process(ILogEntry logEntry)
        {
            this.Count++;
            return true;
        }
    }
}
