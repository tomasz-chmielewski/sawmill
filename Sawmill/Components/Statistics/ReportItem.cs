using System;

namespace Sawmill.Components.Statistics
{
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
