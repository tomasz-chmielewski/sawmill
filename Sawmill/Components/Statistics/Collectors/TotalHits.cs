using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System.Globalization;

namespace Sawmill.Components.Statistics.Collectors
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

        public bool Process(ILogEntry logEntry)
        {
            this.TotalHist++;
            return true;
        }
    }
}
