using Microsoft.Extensions.Options;
using Sawmill.Common.DateAndTime;
using Sawmill.Common.DateAndTime.Extensions;
using Sawmill.Components.Alerts.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Alerts
{
    public class AlertManager : IAlertManager
    {
        public AlertManager(IOptions<AlertManagerOptions> optionsAccessor, IAlertHandler alertHandler)
        {
            this.AlertHandler = alertHandler ?? throw new ArgumentNullException(nameof(alertHandler));

            AlertManagerOptions options = optionsAccessor.Value;
            this.MonitoredPeriodUtc.Duration = TimeSpanEx.FromSecondsInt(options.MonitoredPeriodSeconds);
            this.Delay = TimeSpanEx.FromSecondsInt(options.DelaySeconds);
            this.HitsPerSecondsThreshold = options.HitsPerSecondsThreshold;
        }

        private TimePeriod MonitoredPeriodUtc { get; } = new TimePeriod();
        private TimeSpan Delay { get; }

        private int HitsPerSecondsThreshold { get; }
        private int HitCountThreshold => HitsPerSecondsThreshold * this.MonitoredPeriodUtc.Duration.TotalSecondsAsInt();

        private int MonitoredPeriodHitCount { get; set; }
        private Dictionary<DateTime, int> HistoricalHitCount { get; } = new Dictionary<DateTime, int>();

        private bool HasAlert { get; set; }
        private IAlertHandler AlertHandler { get; }

        public void Initialize(DateTime utcNow)
        {
            this.MonitoredPeriodUtc.Start = this.GetMonitoredPeriodStartUtc(utcNow);
        }

        public int Process(DateTime utcNow, IEnumerable<ILogEntry> logEntries)
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

            this.MoveMonitoredPeriod(utcNow);

            return processedRequests;
        }

        private void MoveMonitoredPeriod(DateTime utcNow)
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

                //Console.WriteLine($"Alert hits: {this.MonitoredPeriodHitCount} ({string.Join(", ", this.HistoricalHitCount.OrderByDescending(x => x.Key).Select(x => x.Value))})");
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

        private bool IsWithinMonitoredPeriod(DateTime utc)
        {
            return utc >= this.MonitoredPeriodUtc.Start && utc < this.MonitoredPeriodUtc.End;
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
