using Sawmill.Models;
using Sawmill.Statistics.Collectors;
using Sawmill.Statistics.Collectors.Abstractions;
using System;
using System.Collections.Generic;

namespace Sawmill.Statistics
{
    public class StatisticsProcessor
    {
        public StatisticsProcessor()
            : this(DateTime.MinValue, DateTime.MaxValue)
        {
        }

        public StatisticsProcessor(DateTime periodStartUtc, DateTime periodEndUtc)
        {
            this.PeriodStartUtc = periodStartUtc;
            this.PeriodEndUtc = periodEndUtc;

            this.collectors = new List<IStatisticsCollector>
            {
                new TotalHits("Hits"),
                new StatusCodes("2xx", 200, 299),
                new StatusCodes("3xx", 300, 399),
                new StatusCodes("4xx", 400, 499),
                new StatusCodes("5xx", 500, 599),
                new UrlSections("Sections")
            };
        }

        private List<IStatisticsCollector> collectors;
        public IEnumerable<IStatisticsCollector> Collectors => this.collectors;

        public DateTime PeriodStartUtc { get; }
        public DateTime PeriodEndUtc { get; }

        public bool Process(LogEntry logEntry)
        {
            if(logEntry.TimeStampUtc < this.PeriodStartUtc || logEntry.TimeStampUtc >= this.PeriodEndUtc)
            {
                return false;
            }

            foreach(var collector in this.collectors)
            {
                collector.Process(logEntry);
            }

            return true;
        }
    }
}
