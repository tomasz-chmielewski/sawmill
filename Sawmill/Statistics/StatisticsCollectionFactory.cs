using Sawmill.Statistics.Collectors;
using Sawmill.Statistics.Collectors.Abstractions;

namespace Sawmill.Statistics
{
    public class StatisticsCollectionFactory
    {
        public StatisticsCollection Create()
        {
            return new StatisticsCollection(
                new IStatisticsCollector[]
                {
                    new TotalHits("Hits"),
                    new StatusCodes("2xx", 200, 299),
                    new StatusCodes("3xx", 300, 399),
                    new StatusCodes("4xx", 400, 499),
                    new StatusCodes("5xx", 500, 599),
                    new UrlSections("Sections")
                });
        }
    }
}
