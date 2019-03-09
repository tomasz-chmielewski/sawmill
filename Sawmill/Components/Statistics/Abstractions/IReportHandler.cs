using Sawmill.Components.Statistics.Collectors.Abstractions;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Statistics.Abstractions
{
    public interface IReportHandler
    {
        void Report(IEnumerable<IStatisticsCollector> globalStatistics, DateTime startUtc, TimeSpan duration, IEnumerable<IStatisticsCollector> periodicStatistics);
    }
}
