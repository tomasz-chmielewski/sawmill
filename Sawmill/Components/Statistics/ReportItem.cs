using System;

namespace Sawmill.Components.Statistics
{
    /// <summary>
    /// Represents a statistics report containing only a single type of statistics (global or periodic).
    /// </summary>
    public class ReportItem
    {
        public ReportItem(DateTime periodStart, DateTime? periodEnd, StatisticsCollection statistics)
        {
            this.PeriodStart = periodStart;
            this.PeriodEnd = periodEnd;
            this.Statistics = statistics;
        }

        public DateTime PeriodStart { get; }
        public DateTime? PeriodEnd { get; }

        public StatisticsCollection Statistics { get; }
    }
}
