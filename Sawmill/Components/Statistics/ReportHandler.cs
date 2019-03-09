using Sawmill.Components.Statistics.Abstractions;
using Sawmill.Components.Statistics.Collectors.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sawmill.Components.Statistics
{
    public class ReportHandler : IReportHandler
    {
        public void Report(IEnumerable<IStatisticsCollector> globalStatistics, DateTime startUtc, TimeSpan duration, IEnumerable<IStatisticsCollector> periodicStatistics)
        {
            var now = DateTime.Now;
            var start = startUtc.ToLocalTime();
            var end = start + duration;

            Console.WriteLine($"[{now.ToLongTimeString()}] Printing statistics for the period {start.ToLongTimeString()} - {end.ToLongTimeString()}");

            this.Report("total", globalStatistics, now);
            this.Report("last ", periodicStatistics, now);
        }

        private void Report(string name, IEnumerable<IStatisticsCollector> statistics, DateTime now)
        {
            if (Console.CursorLeft != 0)
            {
                Console.WriteLine();
            }

            var sb = new StringBuilder();
            sb.Append(FormattableString.Invariant($"[{now.ToString("T")} {name}]"));

            foreach (var collector in statistics)
            {
                sb.Append(" ");
                sb.Append(collector.Name);
                sb.Append("(");
                sb.Append(collector.Value);
                sb.Append(")");
            }

            Console.WriteLine(sb.ToString());
        }
    }
}
