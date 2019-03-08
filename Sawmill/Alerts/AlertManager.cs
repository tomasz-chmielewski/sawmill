using Sawmill.Common.Extensions;
using Sawmill.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sawmill.Alerts
{
    public class AlertManager
    {
        public AlertManager()
        {
            this.AlertHandler = new AlertHandler();
        }

        private DateTime MonitoredPeriodStartUtc { get; set; }
        private TimeSpan MonitoredPeriodDuration { get; } = TimeSpan.FromSeconds(10);
        private DateTime MonitoredPeriodEndUtc => this.MonitoredPeriodStartUtc + this.MonitoredPeriodDuration;

        private TimeSpan Delay => TimeSpan.FromSeconds(1);

        private int HitsPerSecondsThreshold { get; } = 5;
        private int HitCountThreshold => HitsPerSecondsThreshold * MonitoredPeriodDuration.TotalSecondsAsInt();

        private int MonitoredPeriodHitCount { get; set; }
        private Dictionary<DateTime, int> HitCount { get; } = new Dictionary<DateTime, int>();

        private bool HasAlert { get; set; }
        private AlertHandler AlertHandler { get; }

        public void Initialize(DateTime utcNow)
        {
            this.MonitoredPeriodStartUtc = this.GetMonitoredPeriodStartUtc(utcNow);
        }

        public void Process(DateTime utcNow, IEnumerable<LogEntry> logEntries)
        {
            foreach (var logEntry in logEntries)
            {
                if (this.IsWithinMonitoredPeriod(logEntry.TimeStampUtc))
                {
                    // throw new InvalidOperationException("");
                }
                else if (logEntry.TimeStampUtc >= this.MonitoredPeriodEndUtc)
                {
                    var key = logEntry.TimeStampUtc.Floor(TimeSpan.FromSeconds(1));
                    if (this.HitCount.TryGetValue(key, out var value))
                    {
                        this.HitCount[key] = value + 1;
                    }
                    else
                    {
                        this.HitCount[key] = 1;
                    }
                }
            }

            this.MoveMonitoredPeriod(utcNow);
        }

        private void MoveMonitoredPeriod(DateTime utcNow)
        {
            var newStartUtc = this.GetMonitoredPeriodStartUtc(utcNow);

            if (newStartUtc > this.MonitoredPeriodStartUtc)
            {
                while(newStartUtc > this.MonitoredPeriodStartUtc)
                {
                    if(this.HitCount.Count == 0)
                    {
                        this.MonitoredPeriodStartUtc = newStartUtc;
                        break;
                    }

                    if(this.HitCount.Remove(this.MonitoredPeriodStartUtc, out var value))
                    {
                        this.MonitoredPeriodHitCount -= value;
                    }

                    if (this.HitCount.TryGetValue(this.MonitoredPeriodEndUtc, out value))
                    {
                        this.MonitoredPeriodHitCount += value;
                    }

                    this.MonitoredPeriodStartUtc += TimeSpan.FromSeconds(1);

                    this.CheckForAlert(this.MonitoredPeriodEndUtc);
                }

                Console.WriteLine($"Alert hits: {this.MonitoredPeriodHitCount} ({string.Join(", ", this.HitCount.OrderByDescending(x => x.Key).Select(x => x.Value))})");
            }
            else if(newStartUtc < this.MonitoredPeriodStartUtc)
            {
                throw new ArgumentException(nameof(utcNow));
            }
        }

        private DateTime GetMonitoredPeriodStartUtc(DateTime utcNow)
        {
            return utcNow.Floor(TimeSpan.FromSeconds(1)) - this.MonitoredPeriodDuration + TimeSpan.FromSeconds(1) - this.Delay;
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
