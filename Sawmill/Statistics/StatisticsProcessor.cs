using Sawmill.Models;

namespace Sawmill.Statistics
{
    public class StatisticsProcessor
    {
        public int TotalHits { get; private set; }

        public void Process(LogEntry logEntry)
        {
            this.TotalHits++;
        }
    }
}
