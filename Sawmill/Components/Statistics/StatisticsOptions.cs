namespace Sawmill.Components.Statistics
{
    /// <summary>
    /// Statistics manager settings.
    /// </summary>
    public class StatisticsOptions
    {
        /// <summary>
        /// Monitored period duration in seconds.
        /// </summary>
        public int MonitoredPeriodSeconds { get; set; } = 10;

        /// <summary>
        /// Report delay in seconds.
        /// </summary>
        public int ReportDelaySeconds { get; set; } = 1;
    }
}
