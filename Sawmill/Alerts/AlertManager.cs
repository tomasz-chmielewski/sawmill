using Sawmill.Common.DataStructures;
using Sawmill.Common.Extensions;
using Sawmill.Models;
using System;
using System.Linq;

namespace Sawmill.Alerts
{
    public class AlertManager
    {
        public TimeSpan MonitoredPeriodDuration { get; } = TimeSpan.FromSeconds(20);
        public int HitsPerSecondsThreshold { get; } = 5;
        public int HitsCountThreshold => HitsPerSecondsThreshold * MonitoredPeriodDuration.TotalSecondsAsInt();

        private IntervalDictionary<int> Hits { get; set; }

        private int HitsCount { get; set; }
        private bool HasAlert { get; set; }

        public event EventHandler<AlertEventArgs> Alert;

        public void Initialize(DateTime utcNow)
        {
            this.Hits = new IntervalDictionary<int>(utcNow - this.MonitoredPeriodDuration, this.MonitoredPeriodDuration + TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            this.HitsCount = 0;
        }

        public void Process(LogEntry logEntry)
        {
            if(logEntry.TimeStampUtc >= this.Hits.StartUtc && logEntry.TimeStampUtc < this.Hits.EndUtc)
            {
                this.HitsCount++;
                this.Hits[logEntry.TimeStampUtc]++;
            }
        }

        public void Update(DateTime utcNow)
        {
            var utcNowFloor = utcNow.Floor(TimeSpan.FromSeconds(1));
            var startDate = utcNowFloor - this.MonitoredPeriodDuration;

            if(this.Hits == null)
            {
                this.Initialize(startDate);
            }

            var showInfo = startDate != this.Hits.StartUtc;

            if (showInfo)
            {
                Console.WriteLine($"Alert hits: {this.HitsCount} ({string.Join(", ", this.Hits)})");
            }

            this.Hits.MoveStartDate(startDate, hits =>
            {
                this.HitsCount -= hits;
                return 0;
            });

            this.CheckForAlert(utcNowFloor);
        }

        protected virtual void OnAlert(AlertEventArgs e)
        {
            this.Alert?.Invoke(this, e);
        }

        private void CheckForAlert(DateTime timeStamp)
        {
            if (!this.HasAlert && this.HitsCount > this.HitsCountThreshold)
            {
                this.HasAlert = true;
                this.OnAlert(new AlertEventArgs(timeStamp, AlertEvent.Raised, this.HitsCount));
            }
            else if (this.HasAlert && this.HitsCount <= this.HitsCountThreshold)
            {
                this.HasAlert = false;
                this.OnAlert(new AlertEventArgs(timeStamp, AlertEvent.Canceled, this.HitsCount));
            }
        }
    }
}
