using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;

namespace Sawmill.Components.Statistics.Collectors
{
    /// <summary>
    /// Collects statistics related to HTTP response status codes.
    /// </summary>
    public class StatusCodes : IStatisticsCollector
    {
        /// <summary>
        /// Number of collected 2xx status codes.
        /// </summary>
        public int Hits2xx { get; private set; }

        /// <summary>
        /// Number of collected 3xx status codes.
        /// </summary>
        public int Hits3xx { get; private set; }

        /// <summary>
        /// Number of collected 4xx status codes.
        /// </summary>
        public int Hits4xx { get; private set; }

        /// <summary>
        /// Number of collected 4xx status codes.
        /// </summary>
        public int Hits5xx { get; private set; }

        /// <summary>
        /// Process the specified log entry.
        /// </summary>
        /// <param name="logEntry">Log entry to process.</param>
        /// <returns>true if the entry was processed or false otherwise.</returns>
        public bool Process(ILogEntry logEntry)
        {
            var statusCode = logEntry.Status;

            if (statusCode < 200)
            {
                return false;
            }
            else if(statusCode < 300)
            {
                this.Hits2xx++;
                return true;
            }
            else if(statusCode < 400)
            {
                this.Hits3xx++;
                return true;
            }
            else if (statusCode < 500)
            {
                this.Hits4xx++;
                return true;
            }
            else if (statusCode < 600)
            {
                this.Hits5xx++;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
