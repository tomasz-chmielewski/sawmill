using Sawmill.Application.Abstractions;
using Sawmill.Common.Extensions;
using Sawmill.Components.Alerts.Abstractions;
using Sawmill.Components.Statistics.Abstractions;
using Sawmill.Models;
using Sawmill.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Sawmill.Application
{
    public sealed class SawmillApplication : ISawmillApplication
    {
        public SawmillApplication(IAlertManager alertManager, IStatisticsManager statistiscManager)
        {
            this.LogEntryProvider = new LogEntryProvider();

            this.StatisticsManager = statistiscManager ?? throw new ArgumentNullException(nameof(statistiscManager));
            this.AlertManager = alertManager ?? throw new ArgumentNullException(nameof(alertManager));
        }

        private TimeSpan FetchInterval { get; } = TimeSpanEx.FromMillisecondsInt(500);

        private LogEntryProvider LogEntryProvider { get; }

        private IStatisticsManager StatisticsManager { get; }
        private IAlertManager AlertManager { get; }

        public void Dispose()
        {
            this.LogEntryProvider.Dispose();
        }

        public void Run(CancellationToken cancellationToken)
        {
            Initialize();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var logEntries = this.Fetch().ToList();
                var utcNow = DateTime.UtcNow;

                this.Process(utcNow, logEntries);

                if (logEntries.Count == 0)
                {
                    this.WaitForData();
                }
            }
        }

        private void Initialize()
        {
            var utcNow = DateTime.UtcNow;

            this.StatisticsManager.Initialize(utcNow);
            this.AlertManager.Initialize(utcNow);
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

        private IEnumerable<LogEntry> Fetch()
        {
            try
            {
                return this.TryFetch();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return Enumerable.Empty<LogEntry>();
            }
        }

        private IEnumerable<LogEntry> TryFetch()
        {
            for (var i = 0; i < 100; i++)
            {
                var logEntry = this.LogEntryProvider.GetEntry();
                if(logEntry == null)
                {
                    break;                    
                }

                yield return logEntry;
            }
        }

        private void Process(DateTime utcNow, IEnumerable<LogEntry> logEntries)
        {
            this.StatisticsManager.Process(utcNow, logEntries);
            this.AlertManager.Process(utcNow, logEntries);
        }
    }
}
