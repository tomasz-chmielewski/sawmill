using Sawmill.Components.Statistics.Collectors;
using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System.Collections;
using System.Collections.Generic;

namespace Sawmill.Components.Statistics
{
    /// <summary>
    /// Represents a collection of statistics collectors.
    /// </summary>
    public class StatisticsCollection : IEnumerable<IStatisticsCollector>
    {
        public TotalHits Hits { get; } = new TotalHits();
        public StatusCodes StatusCodes { get; } = new StatusCodes();
        public UrlSections UrlSections { get; } = new UrlSections();

        /// <summary>
        /// Process the specified log entry.
        /// </summary>
        /// <param name="logEntry">Log entry to process.</param>
        /// <returns>true if the entry was processed or false otherwise.</returns>
        public bool Process(ILogEntry logEntry)
        {
            var isProcessed = false;
            foreach (var collector in this)
            {
                isProcessed |= collector.Process(logEntry);
            }

            return isProcessed;
        }

        public IEnumerator<IStatisticsCollector> GetEnumerator()
        {
            yield return this.Hits;
            yield return this.StatusCodes;
            yield return this.UrlSections;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
