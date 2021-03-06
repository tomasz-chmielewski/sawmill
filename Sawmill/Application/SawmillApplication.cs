﻿using Sawmill.Application.Abstractions;
using Sawmill.Common.Console;
using Sawmill.Common.DateAndTime.Extensions;
using Sawmill.Components.Alerts;
using Sawmill.Components.Alerts.Abstractions;
using Sawmill.Components.Providers.Abstractions;
using Sawmill.Components.Statistics;
using Sawmill.Components.Statistics.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Sawmill.Application
{
    public sealed class SawmillApplication : ISawmillApplication
    {
        public SawmillApplication(
            ILogEntryProvider logEntryProvider,
            IAlertManager alertManager,
            IStatisticsManager statistiscManager)
        {
            this.StatisticsManager = statistiscManager ?? throw new ArgumentNullException(nameof(statistiscManager));
            this.AlertManager = alertManager ?? throw new ArgumentNullException(nameof(alertManager));
            this.LogEntryProvider = logEntryProvider ?? throw new ArgumentNullException(nameof(logEntryProvider));
        }

        private TimeSpan TryOpenProviderDelay { get; } = TimeSpanEx.FromMillisecondsInt(1000);
        private TimeSpan FetchInterval { get; } = TimeSpanEx.FromMillisecondsInt(100);
        private int BatchSize { get; } = 1000;

        private ILogEntryProvider LogEntryProvider { get; }
        private IStatisticsManager StatisticsManager { get; }
        private IAlertManager AlertManager { get; }

        public void Dispose()
        {
            this.LogEntryProvider.Dispose();
        }

        public void Run(CancellationToken cancellationToken)
        {
            try
            {
                this.WaitForProvider(cancellationToken);
                this.Initialize();

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var logEntries = this.Fetch();

                    var utcNow = DateTime.UtcNow;
                    this.Process(logEntries);
                    this.MoveMonitoredPeriod(utcNow);

                    if (logEntries.Count == 0)
                    {
                        this.WaitForData();
                    }
                }
            }
            finally
            {
                this.LogEntryProvider.Close();
            }
        }

        private void Initialize()
        {
            var utcNow = DateTime.UtcNow;

            this.StatisticsManager.Initialize(utcNow);
            this.AlertManager.Initialize(utcNow);
        }

        private void WaitForProvider(CancellationToken cancellationToken)
        {
            while(true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    this.LogEntryProvider.Open();
                    ConsoleEx.WriteLine($"Reading from \"{this.LogEntryProvider.Path}\"");
                    return;
                }
                catch(IOException e) when (e is FileNotFoundException || e is DirectoryNotFoundException || e is DriveNotFoundException)
                {
                    ConsoleEx.WriteLine(e.Message);
                    Thread.Sleep(this.TryOpenProviderDelay);
                }
            }
        }

        private void WaitForData()
        {
            var utcNow = DateTime.UtcNow;
            var nextFetchTime = utcNow.Ceiling(this.FetchInterval);
            var millisecondsToWait = (nextFetchTime - utcNow).TotalMillisecondsAsInt() + 1;

            if (millisecondsToWait > 0)
            {
                Thread.Sleep(millisecondsToWait);
            }
        }

        private IReadOnlyCollection<ILogEntry> Fetch()
        {
            var entries = new List<ILogEntry>();

            for (var i = 0; i < this.BatchSize; i++)
            {
                var logEntry = this.LogEntryProvider.GetEntry();
                if (logEntry == null)
                {
                    break;
                }

                if(entries.Capacity == 0)
                {
                    entries.Capacity = this.BatchSize;
                }
                entries.Add(logEntry);
            }

            return entries;
        }

        private void Process(IReadOnlyCollection<ILogEntry> logEntries)
        {
            var processedLogs = this.StatisticsManager.Process(logEntries);
            if(processedLogs != logEntries.Count)
            {
                this.HandleWarning($"Statistics manager has rejected {logEntries.Count - processedLogs} logs. Try to increase {nameof(StatisticsOptions.ReportDelaySeconds)} to prevent it.");
            }

            processedLogs = this.AlertManager.Process(logEntries);
            if (processedLogs != logEntries.Count)
            {
                this.HandleWarning($"Alert manager has rejected {logEntries.Count - processedLogs} logs. Try to increase {nameof(AlertManagerOptions.DelaySeconds)} to prevent it.");
            }
        }

        private void MoveMonitoredPeriod(DateTime utcNow)
        {
            this.StatisticsManager.MoveMonitoredPeriod(utcNow);
            this.AlertManager.MoveMonitoredPeriod(utcNow);
        }

        private void HandleWarning(string message)
        {
            ConsoleEx.ColorWrite(ConsoleColor.Yellow, "Warning: ");
            ConsoleEx.WriteLine(message);
        }
    }
}
