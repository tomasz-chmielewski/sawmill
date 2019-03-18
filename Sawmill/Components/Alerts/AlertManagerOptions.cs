namespace Sawmill.Components.Alerts
{
    /// <summary>
    /// Alert manager settings.
    /// </summary>
    public class AlertManagerOptions
    {
        /// <summary>
        /// Monitored period duration in seconds.
        /// </summary>
        public int MonitoredPeriodSeconds { get; set; } = 120;

        /// <summary>
        /// <see cref="AlertManager"/> delay in seconds.
        /// </summary>
        public int DelaySeconds { get; set; } = 1;

        /// <summary>
        /// Alert threshold expressed in average hits per seconds.
        /// </summary>
        public int HitsPerSecondsThreshold { get; set; } = 10;
    }
}
