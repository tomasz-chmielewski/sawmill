using Sawmill.Components.Statistics.Abstractions;
using Sawmill.Components.Statistics.Collectors;
using Sawmill.Components.Statistics.Collectors.Abstractions;

namespace Sawmill.Components.Statistics
{
    public class StatisticsCollectionFactory : IStatisticsCollectionFactory
    {
        public IStatisticsCollection Create()
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
