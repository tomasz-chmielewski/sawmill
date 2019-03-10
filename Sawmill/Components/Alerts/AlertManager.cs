using Microsoft.Extensions.Options;
using Sawmill.Common.Extensions;
using Sawmill.Components.Alerts.Abstractions;
using Sawmill.Models.Abstractions;
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
            this.MonitoredPeriodDuration = TimeSpanEx.FromSecondsInt(options.MonitoredPeriodSeconds);
            this.Delay = TimeSpanEx.FromSecondsInt(options.DelaySeconds);
            this.HitsPerSecondsThreshold = options.HitsPerSecondsThreshold;
        }

        // TODO: Create class Period { Start, Duction, End } ??
        private DateTime MonitoredPeriodStartUtc { get; set; }
        private TimeSpan MonitoredPeriodDuration { get; }
        private DateTime MonitoredPeriodEndUtc => this.MonitoredPeriodStartUtc + this.MonitoredPeriodDuration;

        private TimeSpan Delay { get; }

        private int HitsPerSecondsThreshold { get; }
        private int HitCountThreshold => HitsPerSecondsThreshold * MonitoredPeriodDuration.TotalSecondsAsInt();

        private int MonitoredPeriodHitCount { get; set; }
        private Dictionary<DateTime, int> HistoricalHitCount { get; } = new Dictionary<DateTime, int>();

        private bool HasAlert { get; set; }
        private IAlertHandler AlertHandler { get; }

        public void Initialize(DateTime utcNow)
        {
            this.MonitoredPeriodStartUtc = this.GetMonitoredPeriodStartUtc(utcNow);
        }

        public int Process(DateTime utcNow, IEnumerable<ILogEntry> logEntries)
        {
            var processedRequests = 0;

            foreach (var logEntry in logEntries)
            {
                if (logEntry.TimeStampUtc >= this.MonitoredPeriodEndUtc)
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

            if (newStartUtc > this.MonitoredPeriodStartUtc)
            {
                while(newStartUtc > this.MonitoredPeriodStartUtc)
                {
                    if(this.HistoricalHitCount.Count == 0)
                    {
                        this.MonitoredPeriodStartUtc = newStartUtc;
                        break;
                    }

                    if(this.HistoricalHitCount.Remove(this.MonitoredPeriodStartUtc, out var value))
                    {
                        this.MonitoredPeriodHitCount -= value;
                    }

                    if (this.HistoricalHitCount.TryGetValue(this.MonitoredPeriodEndUtc, out value))
                    {
                        this.MonitoredPeriodHitCount += value;
                    }

                    this.MonitoredPeriodStartUtc += TimeSpanEx.FromSecondsInt(1);

                    this.CheckForAlert(this.MonitoredPeriodEndUtc);
                }

                //Console.WriteLine($"Alert hits: {this.MonitoredPeriodHitCount} ({string.Join(", ", this.HistoricalHitCount.OrderByDescending(x => x.Key).Select(x => x.Value))})");
            }
            else if(newStartUtc < this.MonitoredPeriodStartUtc)
            {
                throw new InvalidOperationException("Cannot move the monitored period to the past");
            }
        }

        private DateTime GetMonitoredPeriodStartUtc(DateTime utcNow)
        {
            return utcNow.FloorSeconds(1) - this.MonitoredPeriodDuration - this.Delay;
        }

        private bool IsWithinMonitoredPeriod(DateTime utc)
        {
            return utc >= this.MonitoredPeriodStartUtc && utc < this.MonitoredPeriodEndUtc;
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
