namespace Sawmill.Components.Alerts
{
    public class AlertManagerOptions
    {
        public int MonitoredPeriodSeconds { get; set; } = 120;
        public int DelaySeconds { get; set; } = 1;
        public int HitsPerSecondsThreshold { get; set; } = 10;
    }
}
