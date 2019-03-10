using Sawmill.Common.DateAndTime;
using Sawmill.Common.DateAndTime.Extensions;
using Sawmill.Components.Statistics.Abstractions;
using Sawmill.Models.Abstractions;
using System;
using System.Collections.Generic;

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

        private DateTime GlobalStatisticsStartUtc { get; set; }

        // TODO: Create class Period { Start, Duction, End } ??
        private DateTime MonitoredPeriodStartUtc { get; set; }
        private TimeSpan MonitoredPeriodDuration { get; } = TimeSpanEx.FromSecondsInt(10);
        private DateTime MonitoredPeriodEndUtc => this.MonitoredPeriodStartUtc + this.MonitoredPeriodDuration;
        private DateTime ReportTimeUtc => this.MonitoredPeriodEndUtc + this.Delay;

        private TimeSpan Delay => TimeSpanEx.FromSecondsInt(1);

        private IStatisticsCollection GlobalStatistics { get; }
        private Dictionary<DateTime, IStatisticsCollection> PeriodicStatistics { get; } = new Dictionary<DateTime, IStatisticsCollection>();

        private IReportHandler ReportHandler { get; }
        private IStatisticsCollectionFactory StatisticsFactory { get; }

        public void Initialize(DateTime utcNow)
        {
            this.MonitoredPeriodStartUtc = this.GetMonitoredPeriodStartUtc(utcNow);
            this.GlobalStatisticsStartUtc = this.MonitoredPeriodStartUtc;
        }

        public int Process(DateTime utcNow, IEnumerable<ILogEntry> logEntries)
        {
            var processedRequests = 0;

            foreach (var logEntry in logEntries)
            {
                if(logEntry.TimeStampUtc >= this.MonitoredPeriodStartUtc)
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

                        //Console.WriteLine($"[{utcNow.ToLocalTime().ToLongTimeString()}] Statistics history: {string.Join(", ", this.PeriodicStatistics.OrderBy(x => x.Key).Select(x => $"[{x.Key.ToLongTimeString()}-{(x.Key + this.MonitoredPeriodDuration).ToLongTimeString()}]"))}");
                    }

                    processedRequests++;
                }
            }

            this.UpdateMonitoredPeriod(utcNow);

            return processedRequests;
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

                    //Console.WriteLine($"[{utcNow.ToLocalTime().ToLongTimeString()}] Statistics history: {string.Join(", ", this.PeriodicStatistics.OrderBy(x => x.Key).Select(x => $"[{x.Key.ToLongTimeString()}-{(x.Key + this.MonitoredPeriodDuration).ToLongTimeString()}]"))}");
                }
            }
            else if (utcNow < this.MonitoredPeriodStartUtc)
            {
                throw new InvalidOperationException("Cannot move the monitored period to the past");
            }
        }

        private void Report(DateTime utcNow)
        {
            var periodStartUtc = utcNow.Floor(this.MonitoredPeriodDuration) - this.MonitoredPeriodDuration;

            var periodicStatistics = this.PeriodicStatistics.TryGetValue(periodStartUtc, out var value)
                ? value 
                : this.StatisticsFactory.Create();

            this.ReportHandler.Report(this.GlobalStatisticsStartUtc, this.GlobalStatistics);
            this.ReportHandler.Report(new TimePeriod(periodStartUtc, this.MonitoredPeriodDuration), periodicStatistics);
        }

        private DateTime GetMonitoredPeriodStartUtc(DateTime utcNow)
        {
            return (utcNow - this.Delay).Floor(this.MonitoredPeriodDuration);
        }
    }
}
