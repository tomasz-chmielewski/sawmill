using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System.Collections.Generic;

namespace Sawmill.Components.Statistics.Abstractions
{
    /// <summary>
    /// Represents a collection of statistics collectors.
    /// </summary>
    public interface IStatisticsCollection : IEnumerable<IStatisticsCollector>
    {
        /// <summary>
        /// Process the specified log entry.
        /// </summary>
        /// <param name="logEntry">Log entry to process.</param>
        /// <returns>true if the entry was processed or false otherwise.</returns>
        bool Process(ILogEntry logEntry);
    }
}
