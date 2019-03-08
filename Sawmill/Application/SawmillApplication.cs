using Sawmill.Alerts;
using Sawmill.Common.Extensions;
using Sawmill.Models;
using Sawmill.Providers;
using Sawmill.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sawmill.Application
{
    public class SawmillApplication : IDisposable
    {
        public SawmillApplication()
        {
            this.GlobalStatistics = new StatisticsProcessor();
            this.LogEntryProvider = new LogEntryProvider();

            this.AlertManager = new AlertManager();
        }

        private TimeSpan FetchInterval { get; } = TimeSpan.FromMilliseconds(500);
        private TimeSpan ReportDelay { get; } = TimeSpan.FromMilliseconds(1000);
        private TimeSpan ReportInterval { get; } = TimeSpan.FromSeconds(2);

        private DateTime NextReportTimeUtc { get; set; }
        private DateTime MinAcceptableTimeStampUtc { get; set; }

        private StatisticsProcessor GlobalStatistics { get; }
        private List<StatisticsProcessor> PeriodicStatistics { get; set; } = new List<StatisticsProcessor>();

        private LogEntryProvider LogEntryProvider { get; }

        private AlertManager AlertManager { get; }

        public void Dispose()
        {
            this.LogEntryProvider.Dispose();
        }

        public void Run(CancellationToken cancellationToken)
        {
            InitializeRun();

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

                this.ReportIfRequired(utcNow);

                //Console.WriteLine($"NowUtc: {utcNow.ToString("mm:ss.fff")}, NextReportTime: {this.NextReportTimeUtc.ToString("mm:ss.fff")}, {string.Join(", ", this.PeriodicStatistics.Select(s => $"{s.PeriodStartUtc.ToString("mm:ss.fff")}-{s.PeriodEndUtc.ToString("mm:ss.fff")}"))}");
            }
        }

        private void InitializeRun()
        {
            var utcNow = DateTime.UtcNow;
            this.UpdateNextReportTime(utcNow);

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

                var utcNow = DateTime.UtcNow;
                this.UpdateNextReportTime(utcNow);
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

        private void UpdateNextReportTime(DateTime utcNow)
        {
            this.MinAcceptableTimeStampUtc = utcNow.Floor(this.ReportInterval);
            this.NextReportTimeUtc = this.MinAcceptableTimeStampUtc + this.ReportInterval + this.ReportDelay;

            this.PeriodicStatistics = this.PeriodicStatistics.Where(s => s.PeriodEndUtc > this.MinAcceptableTimeStampUtc).ToList();
        }

        private void Process(DateTime utcNow, IEnumerable<LogEntry> logEntries)
        {
            //Console.Write('.');
            foreach (var logEntry in logEntries)
            {
                this.GlobalStatistics.Process(logEntry);

                var isProcessed = false;
                foreach (var statistics in this.PeriodicStatistics)
                {
                    isProcessed |= statistics.Process(logEntry);
                }

                if (!isProcessed && logEntry.TimeStampUtc >= this.MinAcceptableTimeStampUtc)
                {
                    var periodStartUtc = logEntry.TimeStampUtc.Floor(this.ReportInterval);
                    var periodEndUtc = logEntry.TimeStampUtc.Ceiling(this.ReportInterval);
                    var statistics = new StatisticsProcessor(periodStartUtc, periodEndUtc);

                    this.PeriodicStatistics.Add(statistics);

                    statistics.Process(logEntry);
                }
            }

            this.AlertManager.Process(utcNow, logEntries);
        }

        private void ReportIfRequired(DateTime utcNow)
        {
            if(this.NextReportTimeUtc <= utcNow)
            {
                this.Report();
                this.UpdateNextReportTime(utcNow);
            }
        }

        private void Report()
        {
            var now = DateTime.Now;
            var utcNow = now.ToUniversalTime();

            var currentPeriodStartUtc = utcNow.Floor(this.ReportInterval);

            var periodicStatistics = 
                this.PeriodicStatistics.FirstOrDefault(s => s.PeriodEndUtc == currentPeriodStartUtc) ?? 
                new StatisticsProcessor();

            this.Report("total", this.GlobalStatistics, now);
            this.Report("last ", periodicStatistics, now);
        }

        private void Report(string name, StatisticsProcessor statistics, DateTime now)
        {
            return;

            if (Console.CursorLeft != 0)
            {
                Console.WriteLine();
            }

            var sb = new StringBuilder();
            sb.Append(FormattableString.Invariant($"[{now.ToString("T")} {name}]"));

            foreach (var collector in statistics.Collectors)
            {
                sb.Append(" ");
                sb.Append(collector.Name);
                sb.Append("(");
                sb.Append(collector.Value);
                sb.Append(")");
            }

            Console.WriteLine(sb.ToString());
        }
    }
}
