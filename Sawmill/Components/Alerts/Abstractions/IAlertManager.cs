﻿using Sawmill.Models;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Alerts.Abstractions
{
    public interface IAlertManager
    {
        void Initialize(DateTime utcNow);
        void Process(DateTime utcNow, IEnumerable<LogEntry> logEntries);
    }
}
