using Sawmill.Common.DateAndTime;
using Sawmill.Components.Statistics.Abstractions;
using Sawmill.Components.Statistics.Collectors.Abstractions;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Statistics
{
    public class ReportHandler : IReportHandler
    {
        public void Report(DateTime start, IEnumerable<IStatisticsCollector> statistics)
        {
            this.Report(start, null, statistics);
        }

        public void Report(TimePeriod period, IEnumerable<IStatisticsCollector> statistics)
        {
            this.Report(period.Start, period.End, statistics);
        }

        private void Report(DateTime start, DateTime? end, IEnumerable<IStatisticsCollector> statistics)
        {
            if (Console.CursorLeft != 0)
            {
                Console.WriteLine();
            }

            var startString = start.ToLongTimeString();
            var endString = end != null ? end.Value.ToLongTimeString() : new string(' ', startString.Length);

            var color = Console.ForegroundColor;
            try
            {
                Console.Write($"[{startString}-{endString}]");

                foreach (var collector in statistics)
                {
                    Console.Write(", ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(collector.Name);
                    Console.ForegroundColor = color;
                    Console.Write(": ");
                    Console.Write(collector.Value);
                }
            }
            finally
            {
                Console.ForegroundColor = color;
            }
        }
    }
}
