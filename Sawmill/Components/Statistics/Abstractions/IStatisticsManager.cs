using Sawmill.Models.Abstractions;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Statistics.Abstractions
{
    public interface IStatisticsManager
    {
        void Initialize(DateTime utcNow);
        int Process(DateTime utcNow, IEnumerable<ILogEntry> logEntries);
    }
}
