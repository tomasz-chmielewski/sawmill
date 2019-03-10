namespace Sawmill.Components.Alerts
{
    public class AlertManagerOptions
    {
        public int MonitoredPeriodSeconds { get; set; } = 120;
        public int DelaySeconds { get; set; } = 0;
        public int HitsPerSecondsThreshold { get; set; } = 10;
    }
}
