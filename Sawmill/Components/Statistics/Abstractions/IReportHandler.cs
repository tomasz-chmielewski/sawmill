using Sawmill.Common.DateAndTime;
using Sawmill.Components.Statistics.Collectors.Abstractions;
using System;
using System.Collections.Generic;

namespace Sawmill.Components.Statistics.Abstractions
{
    public interface IReportHandler
    {
        void Report(DateTime start, IEnumerable<IStatisticsCollector> statistics);
        void Report(TimePeriod period, IEnumerable<IStatisticsCollector> statistics);
    }
}
