using Sawmill.Data.Models.Abstractions;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Alerts.Abstractions
{
    /// <summary>
    /// Represents an object that provides alert-related application logic.
    /// </summary>
    public interface IAlertManager
    {
        /// <summary>
        /// Initialize the monitored period according to the specified current time in UTC.
        /// </summary>
        /// <param name="utcNow">Current time in UTC.</param>
        void Initialize(DateTime utcNow);

        /// <summary>
        /// Process the specified log entries.
        /// </summary>
        /// <param name="logEntries">Log entries to process.</param>
        /// <returns>Number of processed entries. It may be less then the number of specified entries if some of the entries are rejected.</returns>
        int Process(IEnumerable<ILogEntry> logEntries);

        /// <summary>
        /// Moves the monitored period according to the specified current time in UTC.
        /// </summary>
        /// <param name="utcNow">Current time in UTC.</param>
        void MoveMonitoredPeriod(DateTime utcNow);
    }
}
