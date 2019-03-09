using Sawmill.Models;
using Sawmill.Statistics.Collectors.Abstractions;
using System.Globalization;

namespace Sawmill.Statistics.Collectors
{
    public class TotalHits : IStatisticsCollector
    {
        public TotalHits(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
        public string Value => TotalHist.ToString(CultureInfo.InvariantCulture);

        private int TotalHist { get; set; }

        public bool Process(LogEntry logEntry)
        {
            this.TotalHist++;
            return true;
        }
    }
}
