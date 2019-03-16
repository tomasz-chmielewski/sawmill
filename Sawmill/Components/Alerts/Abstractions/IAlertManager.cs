using Sawmill.Data.Models.Abstractions;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Alerts.Abstractions
{
    public interface IAlertManager
    {
        void Initialize(DateTime utcNow);
        int Process(IEnumerable<ILogEntry> logEntries);
        void MoveMonitoredPeriod(DateTime utcNow);
    }
}
