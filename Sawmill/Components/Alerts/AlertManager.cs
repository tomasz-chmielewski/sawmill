using Microsoft.Extensions.Options;
using Sawmill.Common.DateAndTime;
using Sawmill.Common.DateAndTime.Extensions;
using Sawmill.Components.Alerts.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Alerts
{
    /// <summary>
    /// Provides alert-related application logic.
    /// </summary>
    public class AlertManager : IAlertManager
    {
        public AlertManager(IOptions<AlertManagerOptions> optionsAccessor, IAlertHandler alertHandler)
        {
            this.AlertHandler = alertHandler ?? throw new ArgumentNullException(nameof(alertHandler));

            var options = optionsAccessor.Value;
            this.MonitoredPeriodUtc.Duration = TimeSpanEx.FromSecondsInt(options.MonitoredPeriodSeconds);
            this.Delay = TimeSpanEx.FromSecondsInt(options.DelaySeconds);
            this.HitsPerSecondsThreshold = options.HitsPerSecondsThreshold;
        }

        private TimePeriod MonitoredPeriodUtc { get; } = new TimePeriod();
        private TimeSpan Delay { get; }

        private int HitsPerSecondsThreshold { get; }
        private int HitCountThreshold => this.HitsPerSecondsThreshold * this.MonitoredPeriodUtc.Duration.TotalSecondsAsInt();

        private int MonitoredPeriodHitCount { get; set; }
        private Dictionary<DateTime, int> HistoricalHitCount { get; } = new Dictionary<DateTime, int>();

        private bool HasAlert { get; set; }
        private IAlertHandler AlertHandler { get; }

        /// <summary>
        /// Initialize the monitored period according to the specified current time in UTC.
        /// </summary>
        /// <param name="utcNow">Current time in UTC.</param>
        public void Initialize(DateTime utcNow)
        {
            this.MonitoredPeriodUtc.Start = this.GetMonitoredPeriodStartUtc(utcNow);
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
                if (logEntry.TimeStampUtc >= this.MonitoredPeriodUtc.End)
                {
                    var key = logEntry.TimeStampUtc.FloorSeconds(1);
                    if (this.HistoricalHitCount.TryGetValue(key, out var value))
                    {
                        this.HistoricalHitCount[key] = value + 1;
                    }
                    else
                    {
                        this.HistoricalHitCount[key] = 1;
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
            var newStartUtc = this.GetMonitoredPeriodStartUtc(utcNow);

            if (newStartUtc > this.MonitoredPeriodUtc.Start)
            {
                while(newStartUtc > this.MonitoredPeriodUtc.Start)
                {
                    if(this.HistoricalHitCount.Count == 0)
                    {
                        this.MonitoredPeriodUtc.Start = newStartUtc;
                        break;
                    }

                    if(this.HistoricalHitCount.Remove(this.MonitoredPeriodUtc.Start, out var value))
                    {
                        this.MonitoredPeriodHitCount -= value;
                    }

                    if (this.HistoricalHitCount.TryGetValue(this.MonitoredPeriodUtc.End, out value))
                    {
                        this.MonitoredPeriodHitCount += value;
                    }

                    this.MonitoredPeriodUtc.Start += TimeSpanEx.FromSecondsInt(1);

                    this.CheckForAlert(this.MonitoredPeriodUtc.End);
                }
            }
            else if(newStartUtc < this.MonitoredPeriodUtc.Start)
            {
                throw new InvalidOperationException("Cannot move the monitored period to the past");
            }
        }

        private DateTime GetMonitoredPeriodStartUtc(DateTime utcNow)
        {
            return utcNow.FloorSeconds(1) - this.MonitoredPeriodUtc.Duration - this.Delay;
        }

        private void CheckForAlert(DateTime timeStamp)
        {
            if (!this.HasAlert && this.MonitoredPeriodHitCount > this.HitCountThreshold)
            {
                this.HasAlert = true;
                this.AlertHandler.RaiseAlert(timeStamp, this.MonitoredPeriodHitCount);
            }
            else if (this.HasAlert && this.MonitoredPeriodHitCount <= this.HitCountThreshold)
            {
                this.HasAlert = false;
                this.AlertHandler.RecoverFromAlert(timeStamp, this.MonitoredPeriodHitCount);
            }
        }
    }
}
