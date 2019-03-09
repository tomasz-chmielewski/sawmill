using Sawmill.Common.Extensions;
using Sawmill.Components.Statistics.Abstractions;
using Sawmill.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sawmill.Components.Statistics
{
    public class StatisticsManager : IStatisticsManager
    {
        public StatisticsManager(IReportHandler reportHandler, IStatisticsCollectionFactory statisticsFactory)
        {
            this.ReportHandler = reportHandler ?? throw new ArgumentNullException(nameof(reportHandler));
            this.StatisticsFactory = statisticsFactory ?? throw new ArgumentNullException(nameof(statisticsFactory));

            this.GlobalStatistics = this.StatisticsFactory.Create();
        }

        // TODO: Create class Period { Start, Duction, End } ??
        private DateTime GlobalStatisticsStartUtc { get; set; }

        private DateTime MonitoredPeriodStartUtc { get; set; }
        private TimeSpan MonitoredPeriodDuration { get; } = TimeSpan.FromSeconds(10);
        private DateTime MonitoredPeriodEndUtc => this.MonitoredPeriodStartUtc + this.MonitoredPeriodDuration;
        private DateTime ReportTimeUtc => this.MonitoredPeriodEndUtc + this.Delay;

        private TimeSpan Delay => TimeSpan.FromSeconds(1);

        private IStatisticsCollection GlobalStatistics { get; }
        private Dictionary<DateTime, IStatisticsCollection> PeriodicStatistics { get; } = new Dictionary<DateTime, IStatisticsCollection>();

        private IReportHandler ReportHandler { get; }
        private IStatisticsCollectionFactory StatisticsFactory { get; }

        public void Initialize(DateTime utcNow)
        {
            this.MonitoredPeriodStartUtc = this.GetMonitoredPeriodStartUtc(utcNow);
        }

        public void Process(DateTime utcNow, IEnumerable<LogEntry> logEntries)
        {
            foreach (var logEntry in logEntries)
            {
                if(logEntry.TimeStampUtc < this.MonitoredPeriodStartUtc)
                {
                    // log is too old
                    // throw new InvalidOperationException("");
                }
                else
                {
                    this.GlobalStatistics.Process(logEntry);

                    var key = logEntry.TimeStampUtc.Floor(this.MonitoredPeriodDuration);
                    if (this.PeriodicStatistics.TryGetValue(key, out var statistics))
                    {
                        statistics.Process(logEntry);
                    }
                    else
                    {
                        statistics = this.StatisticsFactory.Create();
                        this.PeriodicStatistics[key] = statistics;
                        statistics.Process(logEntry);

                        Console.WriteLine($"[{utcNow.ToLocalTime().ToLongTimeString()}] Statistics history: {string.Join(", ", this.PeriodicStatistics.OrderBy(x => x.Key).Select(x => $"[{x.Key.ToLongTimeString()}-{(x.Key + this.MonitoredPeriodDuration).ToLongTimeString()}]"))}");
                    }
                }
            }

            this.UpdateMonitoredPeriod(utcNow);
        }

        private void UpdateMonitoredPeriod(DateTime utcNow)
        {
            if (utcNow >= this.ReportTimeUtc)
            {
                this.Report(utcNow);

                var newStartUtc = this.GetMonitoredPeriodStartUtc(utcNow);

                while (this.MonitoredPeriodStartUtc < newStartUtc)
                {
                    if (this.PeriodicStatistics.Count == 0)
                    {
                        this.MonitoredPeriodStartUtc = newStartUtc;
                        break;
                    }

                    this.PeriodicStatistics.Remove(this.MonitoredPeriodStartUtc);
                    this.MonitoredPeriodStartUtc += this.MonitoredPeriodDuration;

                    Console.WriteLine($"[{utcNow.ToLocalTime().ToLongTimeString()}] Statistics history: {string.Join(", ", this.PeriodicStatistics.OrderBy(x => x.Key).Select(x => $"[{x.Key.ToLongTimeString()}-{(x.Key + this.MonitoredPeriodDuration).ToLongTimeString()}]"))}");
                }
            }
            else if (utcNow < this.MonitoredPeriodStartUtc)
            {
                throw new ArgumentException(nameof(utcNow));
            }
        }

        private void Report(DateTime utcNow)
        {
            var periodStartUtc = utcNow.Floor(this.MonitoredPeriodDuration) - this.MonitoredPeriodDuration;

            var periodicStatistics = this.PeriodicStatistics.TryGetValue(periodStartUtc, out var value)
                ? value 
                : this.StatisticsFactory.Create();

            this.ReportHandler.Report(this.GlobalStatistics, periodStartUtc, this.MonitoredPeriodDuration, periodicStatistics);
        }

        private DateTime GetMonitoredPeriodStartUtc(DateTime utcNow)
        {
            return (utcNow - this.Delay).Floor(this.MonitoredPeriodDuration);
        }
    }
}
