using Microsoft.Extensions.Options;
using Sawmill.Common.DateAndTime;
using Sawmill.Common.DateAndTime.Extensions;
using Sawmill.Components.Statistics.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Statistics
{
    /// <summary>
    /// Provides statistics-related application logic.
    /// </summary>
    public class StatisticsManager : IStatisticsManager
    {
        public StatisticsManager(IOptions<StatisticsOptions> optionsAccessor, IReportHandler reportHandler)
        {
            this.ReportHandler = reportHandler ?? throw new ArgumentNullException(nameof(reportHandler));

            this.GlobalStatistics = new StatisticsCollection();

            var options = optionsAccessor.Value;
            this.MonitoredPeriodUtc.Duration = TimeSpanEx.FromSecondsInt(options.MonitoredPeriodSeconds);
            this.ReportDelay = TimeSpanEx.FromSecondsInt(options.ReportDelaySeconds);
        }

        private TimePeriod MonitoredPeriodUtc { get; } = new TimePeriod();
        private DateTime GlobalStatisticsStartUtc { get; set; }
        private DateTime ReportTimeUtc => this.MonitoredPeriodUtc.End + this.ReportDelay;
        private TimeSpan ReportDelay { get; }

        private StatisticsCollection GlobalStatistics { get; }
        private Dictionary<DateTime, StatisticsCollection> PeriodicStatistics { get; } = new Dictionary<DateTime, StatisticsCollection>();

        private IReportHandler ReportHandler { get; }

        /// <summary>
        /// Initialize the monitored period according to the specified current time in UTC.
        /// </summary>
        public void Initialize(DateTime utcNow)
        {
            this.MonitoredPeriodUtc.Start = this.GetMonitoredPeriodStartUtc(utcNow);
            this.GlobalStatisticsStartUtc = this.MonitoredPeriodUtc.Start;
        }

        /// <summary>
        /// Process the specified log entries.
        /// </summary>
        /// <param name="logEntries">Log entries to process.</param>
        /// <returns>Number of processed entries. It may be less then the number of specified entries if some of the entries are rejected.</returns>
        public int Process(IEnumerable<ILogEntry> logEntries)
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
                        statistics = new StatisticsCollection();
                        this.PeriodicStatistics[key] = statistics;
                        statistics.Process(logEntry);
                    }

                    processedRequests++;
                }
            }

            return processedRequests;
        }

        /// <summary>
        /// Moves the monitored period according to the specified current time in UTC.
        /// </summary>
        /// <param name="utcNow">Current time in UTC.</param>
        /// <exception cref="InvalidOperationException">Attempt to move the monitored period to the past.</exception>
        public void MoveMonitoredPeriod(DateTime utcNow)
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
                }
            }
            else if (utcNow < this.MonitoredPeriodUtc.Start)
            {
                throw new InvalidOperationException("Cannot move the monitored period to the past.");
            }
        }

        private void Report(DateTime utcNow)
        {
            var periodEndUtc = (utcNow - this.ReportDelay).Floor(this.MonitoredPeriodUtc.Duration);
            var periodStartUtc = periodEndUtc - this.MonitoredPeriodUtc.Duration;

            var periodicStatistics = this.PeriodicStatistics.TryGetValue(periodStartUtc, out var value)
                ? value
                : new StatisticsCollection();

            var globalReport = new ReportItem(this.GlobalStatisticsStartUtc, null, this.GlobalStatistics);
            var periodicReport = new ReportItem(periodStartUtc, periodEndUtc, periodicStatistics);

            var report = new Report(globalReport, periodicReport);

            this.ReportHandler.HandleReport(report);
        }

        private DateTime GetMonitoredPeriodStartUtc(DateTime utcNow)
        {
            return (utcNow - this.ReportDelay).Floor(this.MonitoredPeriodUtc.Duration);
        }
    }
}
