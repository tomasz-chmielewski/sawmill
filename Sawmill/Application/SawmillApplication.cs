using Sawmill.Alerts;
using Sawmill.Common.Extensions;
using Sawmill.Models;
using Sawmill.Providers;
using Sawmill.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Sawmill.Application
{
    public class SawmillApplication : IDisposable
    {
        public SawmillApplication()
        {
            this.LogEntryProvider = new LogEntryProvider();

            this.StatisticsManager = new StatisticsManager();
            this.AlertManager = new AlertManager();
        }

        private TimeSpan FetchInterval { get; } = TimeSpan.FromMilliseconds(500);

        private LogEntryProvider LogEntryProvider { get; }

        private StatisticsManager StatisticsManager { get; }
        private AlertManager AlertManager { get; }

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
