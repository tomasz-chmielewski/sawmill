using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;

namespace Sawmill.Components.Statistics.Collectors
{
    /// <summary>
    /// Collects statistics related to HTTP response status codes.
    /// </summary>
    public class StatusCodes : IStatisticsCollector
    {
        private readonly int[] hits = new int[4];

        /// <summary>
        /// Number of collected 2xx status codes.
        /// </summary>
        public int Hits2xx => this.hits[0];

        /// <summary>
        /// Number of collected 3xx status codes.
        /// </summary>
        public int Hits3xx => this.hits[1];

        /// <summary>
        /// Number of collected 4xx status codes.
        /// </summary>
        public int Hits4xx => this.hits[2];

        /// <summary>
        /// Number of collected 4xx status codes.
        /// </summary>
        public int Hits5xx => this.hits[3];

        /// <summary>
        /// Process the specified log entry.
        /// </summary>
        /// <param name="logEntry">Log entry to process.</param>
        /// <returns>true if the entry was processed or false otherwise.</returns>
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
