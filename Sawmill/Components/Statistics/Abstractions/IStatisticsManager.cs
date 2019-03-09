using Sawmill.Models;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Statistics.Abstractions
{
    public interface IStatisticsManager
    {
        void Initialize(DateTime utcNow);
        void Process(DateTime utcNow, IEnumerable<LogEntry> logEntries);
    }
}
