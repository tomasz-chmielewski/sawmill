using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;

namespace Sawmill.Components.Statistics.Collectors
{
    public class StatusCodes : IStatisticsCollector
    {
        private readonly int[] hits = new int[4];

        public int Hits2xx => this.hits[0];
        public int Hits3xx => this.hits[1];
        public int Hits4xx => this.hits[2];
        public int Hits5xx => this.hits[3];

        public bool Process(ILogEntry logEntry)
        {
            var index = (logEntry.Status - 200) / 100;
            if(index < 0 || index >= this.hits.Length)
            {
                return false;
            }

            this.hits[index]++;
            return true;
        }
    }
}
