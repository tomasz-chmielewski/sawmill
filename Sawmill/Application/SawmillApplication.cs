using Sawmill.Common.Extensions;
using Sawmill.Models;
using Sawmill.Providers;
using Sawmill.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sawmill.Application
{
    public class SawmillApplication : IDisposable
    {
        public SawmillApplication()
        {
            this.GlobalStatistics = new StatisticsProcessor();
            this.LogEntryProvider = new LogEntryProvider();
        }

        private TimeSpan FetchInterval { get; } = TimeSpan.FromMilliseconds(100);
        private TimeSpan ReportDelay { get; } = TimeSpan.FromMilliseconds(100);
        private TimeSpan ReportInterval { get; } = TimeSpan.FromSeconds(1);

        private DateTime NextReportTimeUtc { get; set; }
        private DateTime MinAcceptableTimeStampUtc { get; set; }

        private StatisticsProcessor GlobalStatistics { get; }
        private List<StatisticsProcessor> PeriodicStatistics { get; set; } = new List<StatisticsProcessor>();

        private LogEntryProvider LogEntryProvider { get; }

        public void Dispose()
        {
            this.LogEntryProvider.Dispose();
        }

        public void Run(CancellationToken cancellationToken)
        {
            InitializeRun();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var logEntry = this.Fetch();

                if (logEntry != null)
                {
                    this.Process(logEntry);
                }
                else
                {
                    this.WaitForData();
                }

                var utcNow = DateTime.UtcNow;
                this.ReportIfRequired(utcNow);

                //Console.WriteLine($"NowUtc: {utcNow.ToString("mm:ss.fff")}, NextReportTime: {this.NextReportTimeUtc.ToString("mm:ss.fff")}, {string.Join(", ", this.PeriodicStatistics.Select(s => $"{s.PeriodStartUtc.ToString("mm:ss.fff")}-{s.PeriodEndUtc.ToString("mm:ss.fff")}"))}");
            }
        }

        private void InitializeRun()
        {
            var utcNow = DateTime.UtcNow;
            this.UpdateNextReportTime(utcNow);
        }

        private void WaitForData()
        {
            var utcNow = DateTime.UtcNow;
            var nextFetchTime = utcNow.Ceiling(this.FetchInterval);
            var millisecondsToWait = (nextFetchTime - utcNow).TotalMillisecondsAsInt() + 1;

            if (millisecondsToWait > 0)
            {
                Thread.Sleep(millisecondsToWait);
            }
        }

        private LogEntry Fetch()
        {
            try
            {
                return this.LogEntryProvider.GetEntry();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);

                var utcNow = DateTime.UtcNow;
                this.UpdateNextReportTime(utcNow);
                return null;
            }
        }

        private void UpdateNextReportTime(DateTime utcNow)
        {
            this.MinAcceptableTimeStampUtc = utcNow.Floor(this.ReportInterval);
            this.NextReportTimeUtc = this.MinAcceptableTimeStampUtc + this.ReportInterval + this.ReportDelay;

            this.PeriodicStatistics = this.PeriodicStatistics.Where(s => s.PeriodEndUtc > this.MinAcceptableTimeStampUtc).ToList();
        }

        private void Process(LogEntry logEntry)
        {
            //Console.Write('.');
            this.GlobalStatistics.Process(logEntry);

            var isProcessed = false;
            foreach(var statistics in this.PeriodicStatistics)
            {
                isProcessed |= statistics.Process(logEntry);
            }

            if(!isProcessed && logEntry.TimeStampUtc >= this.MinAcceptableTimeStampUtc)
            {
                var periodStartUtc = logEntry.TimeStampUtc.Floor(this.ReportInterval);
                var periodEndUtc = logEntry.TimeStampUtc.Ceiling(this.ReportInterval);
                var statistics = new StatisticsProcessor(periodStartUtc, periodEndUtc);

                this.PeriodicStatistics.Add(statistics);

                statistics.Process(logEntry);
            }
        }

        private void ReportIfRequired(DateTime utcNow)
        {
            if(this.NextReportTimeUtc <= utcNow)
            {
                this.Report();
                this.UpdateNextReportTime(utcNow);
            }
        }

        private void Report()
        {
            var now = DateTime.Now;
            var utcNow = now.ToUniversalTime();

            var currentPeriodStartUtc = utcNow.Floor(this.ReportInterval);

            var periodicStatistics = 
                this.PeriodicStatistics.FirstOrDefault(s => s.PeriodEndUtc == currentPeriodStartUtc) ?? 
                new StatisticsProcessor();

            this.Report("total", this.GlobalStatistics, now);
            this.Report("last ", periodicStatistics, now);
        }

        private void Report(string name, StatisticsProcessor statistics, DateTime now)
        {
            if (Console.CursorLeft != 0)
            {
                Console.WriteLine();
            }

            var sb = new StringBuilder();
            sb.Append(FormattableString.Invariant($"[{now.ToString("T")} {name}]"));

            foreach (var collector in statistics.Collectors)
            {
                sb.Append(" ");
                sb.Append(collector.Name);
                sb.Append("(");
                sb.Append(collector.Value);
                sb.Append(")");
            }

            Console.WriteLine(sb.ToString());
        }
    }
}
