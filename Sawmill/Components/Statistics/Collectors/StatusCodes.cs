using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System.Globalization;

namespace Sawmill.Components.Statistics.Collectors
{
    public class StatusCodes : IStatisticsCollector
    {
        public StatusCodes(string name, int min, int max)
        {
            this.Name = name;
            this.Min = min;
            this.Max = max;
        }

        public string Name { get; }
        public string Value => Hits.ToString(CultureInfo.InvariantCulture);

        private int Min { get; }
        private int Max { get; }
        private int Hits { get; set; }

        public bool Process(ILogEntry logEntry)
        {
            if(logEntry.Status < this.Min || logEntry.Status > this.Max)
            {
                return false;
            }

            this.Hits++;
            return true;
        }
    }
}
