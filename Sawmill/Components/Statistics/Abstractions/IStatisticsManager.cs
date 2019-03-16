using Sawmill.Data.Models.Abstractions;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Statistics.Abstractions
{
    public interface IStatisticsManager
    {
        void Initialize(DateTime utcNow);
        int Process(IEnumerable<ILogEntry> logEntries);
        void MoveMonitoredPeriod(DateTime utcNow);
    }
}
