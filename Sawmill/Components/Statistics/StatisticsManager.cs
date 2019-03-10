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

        private TimePeriod MonitoredPeriodUtc { get; } = new TimePeriod();
        private DateTime GlobalStatisticsStartUtc { get; set; }
        private DateTime ReportTimeUtc => this.MonitoredPeriodUtc.End + this.Delay;
        private TimeSpan Delay => TimeSpanEx.FromSecondsInt(1);

        private IStatisticsCollection GlobalStatistics { get; }
        private Dictionary<DateTime, IStatisticsCollection> PeriodicStatistics { get; } = new Dictionary<DateTime, IStatisticsCollection>();

        private IReportHandler ReportHandler { get; }
        private IStatisticsCollectionFactory StatisticsFactory { get; }

        public void Initialize(DateTime utcNow)
        {
            this.MonitoredPeriodUtc.Start = this.GetMonitoredPeriodStartUtc(utcNow);
            this.GlobalStatisticsStartUtc = this.MonitoredPeriodUtc.Start;
        }

        public int Process(DateTime utcNow, IEnumerable<ILogEntry> logEntries)
        {
            var processedRequests = 0;

            foreach (var logEntry in logEntries)
            {
                if(logEntry.TimeStampUtc >= this.MonitoredPeriodUtc.Start)
                {
                    this.GlobalStatistics.Process(logEntry);

                    var key = logEntry.TimeStampUtc.Floor(this.MonitoredPeriodUtc.Duration);
                    if (this.PeriodicStatistics.TryGetValue(key, out var statistics))
                    {
                        statistics.Process(logEntry);
                    }
                    else
                    {
                        statistics = this.StatisticsFactory.Create();
                        this.PeriodicStatistics[key] = statistics;
                        statistics.Process(logEntry);

                        //Console.WriteLine($"[{utcNow.ToLocalTime().ToLongTimeString()}] Statistics history: {string.Join(", ", this.PeriodicStatistics.OrderBy(x => x.Key).Select(x => $"[{x.Key.ToLongTimeString()}-{(x.Key + this.MonitoredPeriodUtc.Duration).ToLongTimeString()}]"))}");
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

                while (this.MonitoredPeriodUtc.Start < newStartUtc)
                {
                    if (this.PeriodicStatistics.Count == 0)
                    {
                        this.MonitoredPeriodUtc.Start = newStartUtc;
                        break;
                    }

                    this.PeriodicStatistics.Remove(this.MonitoredPeriodUtc.Start);
                    this.MonitoredPeriodUtc.Start += this.MonitoredPeriodUtc.Duration;

                    //Console.WriteLine($"[{utcNow.ToLocalTime().ToLongTimeString()}] Statistics history: {string.Join(", ", this.PeriodicStatistics.OrderBy(x => x.Key).Select(x => $"[{x.Key.ToLongTimeString()}-{(x.Key + this.MonitoredPeriodUtc.Duration).ToLongTimeString()}]"))}");
                }
            }
            else if (utcNow < this.MonitoredPeriodUtc.Start)
            {
                throw new InvalidOperationException("Cannot move the monitored period to the past");
            }
        }

        private void Report(DateTime utcNow)
        {
            var periodStartUtc = utcNow.Floor(this.MonitoredPeriodUtc.Duration) - this.MonitoredPeriodUtc.Duration;

            var periodicStatistics = this.PeriodicStatistics.TryGetValue(periodStartUtc, out var value)
                ? value 
                : this.StatisticsFactory.Create();

            this.ReportHandler.Report(this.GlobalStatisticsStartUtc, this.GlobalStatistics);
            this.ReportHandler.Report(new TimePeriod(periodStartUtc, this.MonitoredPeriodUtc.Duration), periodicStatistics);
        }

        private DateTime GetMonitoredPeriodStartUtc(DateTime utcNow)
        {
            return (utcNow - this.Delay).Floor(this.MonitoredPeriodUtc.Duration);
        }
    }
}
