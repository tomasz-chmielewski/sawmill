using Sawmill.Models;
using Sawmill.Providers;
using Sawmill.Statistics;
using System;
using System.IO;
using System.Threading;

namespace Sawmill.Application
{
    public class SawmillApplication : IDisposable
    {
        public SawmillApplication()
        {
            this.StatisticsCollector = new StatisticsProcessor();
            this.LogEntryProvider = new LogEntryProvider();
        }

        private TimeSpan DelayInterval { get; } = TimeSpan.FromMilliseconds(100);
        private TimeSpan ReportInterval { get; } = TimeSpan.FromSeconds(1);

        private StatisticsProcessor StatisticsCollector { get; }
        private LogEntryProvider LogEntryProvider { get; }

        private DateTime NextReportTimeUtc { get; set; }

        public void Dispose()
        {
            this.LogEntryProvider.Dispose();
        }

        public void Run(CancellationToken cancellationToken)
        {
            this.UpdateNextReportTime();

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
                    this.Delay();
                }

                this.ReportIfRequired();
            }
        }

        private void Delay()
        {
            var timeToNextReport = this.NextReportTimeUtc - DateTime.UtcNow;
            var delayLength = timeToNextReport <= this.DelayInterval
                ? timeToNextReport
                : this.DelayInterval;

            var delayMilliseconds = (int)delayLength.TotalMilliseconds;
            //Console.WriteLine($"Sleeping {delayMilliseconds}ms");
            if (delayMilliseconds > 0)
            {
                Thread.Sleep(delayMilliseconds);
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
                UpdateNextReportTime();
                return null;
            }
        }

        private void UpdateNextReportTime(DateTime? now = null)
        {
            var nowUtc = now != null ? now.Value.ToUniversalTime() : DateTime.UtcNow;
            this.NextReportTimeUtc = nowUtc + this.ReportInterval;
        }

        private void Process(LogEntry logEntry)
        {
            //Console.Write('.');
            this.StatisticsCollector.Process(logEntry);
        }

        private void ReportIfRequired()
        {
            var nowUtc = DateTime.UtcNow;
            if(this.NextReportTimeUtc <= nowUtc)
            {
                this.Report();
                this.UpdateNextReportTime(nowUtc);
            }
        }

        private void Report()
        {
            var now = DateTime.Now;
            if(Console.CursorLeft != 0)
            {
                Console.WriteLine();
            }

            Console.WriteLine(FormattableString.Invariant($"[{now.ToString("T")}] Total hits: {this.StatisticsCollector.TotalHits}"));
        }
    }
}
