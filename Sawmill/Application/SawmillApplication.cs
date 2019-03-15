using Sawmill.Application.Abstractions;
using Sawmill.Common.DateAndTime.Extensions;
using Sawmill.Components.Alerts.Abstractions;
using Sawmill.Components.Providers.Abstractions;
using Sawmill.Components.Statistics.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private TimeSpan OpenProviderDelay { get; } = TimeSpanEx.FromMillisecondsInt(1000);
        private TimeSpan FetchInterval { get; } = TimeSpanEx.FromMillisecondsInt(500);
        private int BatchSize { get; } = 100;

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

                    // TODO: Process line by line and call UpdateMonitoredPeriod() when there is no data
                    var logEntries = this.Fetch().ToList();

                    var utcNow = DateTime.UtcNow;
                    this.Process(utcNow, logEntries);

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
                    Console.WriteLine($"Reading from \"{this.LogEntryProvider.Path}\"");
                    return;
                }
                catch(IOException e) when (e is FileNotFoundException || e is DirectoryNotFoundException || e is DriveNotFoundException)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(this.OpenProviderDelay);
                }
            }
        }

        private void WaitForData()
        {
            // TODO: use FileSystemWatcher class instead of Thread.Sleep()

            var utcNow = DateTime.UtcNow;
            var nextFetchTime = utcNow.Ceiling(this.FetchInterval);
            var millisecondsToWait = (nextFetchTime - utcNow).TotalMillisecondsAsInt() + 1;

            if (millisecondsToWait > 0)
            {
                Thread.Sleep(millisecondsToWait);
            }
        }

        private IEnumerable<ILogEntry> Fetch()
        {
            for (var i = 0; i < this.BatchSize; i++)
            {
                var logEntry = this.LogEntryProvider.GetEntry();
                if(logEntry == null)
                {
                    break;                    
                }

                yield return logEntry;
            }
        }

        private void Process(DateTime utcNow, IEnumerable<ILogEntry> logEntries)
        {
            this.StatisticsManager.Process(utcNow, logEntries);
            this.AlertManager.Process(utcNow, logEntries);
        }
    }
}
