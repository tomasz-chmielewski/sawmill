using System;

namespace Sawmill.Alerts
{
    public class AlertEventArgs : EventArgs
    {
        public AlertEventArgs(DateTime timeStamp, AlertEvent alertEvent, int hitsCount)
        {
            this.TimeStamp = timeStamp;
            this.AlertEvent = alertEvent;
            this.HitsCount = hitsCount;
        }

        public DateTime TimeStamp { get; }
        public AlertEvent AlertEvent { get; }
        public int HitsCount { get; }
    }
}
