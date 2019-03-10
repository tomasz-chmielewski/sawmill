using Sawmill.Models.Abstractions;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Alerts.Abstractions
{
    public interface IAlertManager
    {
        void Initialize(DateTime utcNow);
        int Process(DateTime utcNow, IEnumerable<ILogEntry> logEntries);
    }
}
