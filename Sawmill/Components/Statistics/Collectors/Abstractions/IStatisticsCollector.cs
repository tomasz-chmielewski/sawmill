using Sawmill.Data.Models.Abstractions;

namespace Sawmill.Components.Statistics.Collectors.Abstractions
{
    /// <summary>
    /// Representa an object that collects statistics.
    /// </summary>
    public interface IStatisticsCollector
    {
        /// <summary>
        /// Process the specified log entry.
        /// </summary>
        /// <param name="logEntry">Log entry to process.</param>
        /// <returns>true if the entry was processed or false otherwise.</returns>
        bool Process(ILogEntry logEntry);
    }
}
