using Sawmill.Alerts;
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

            this.AlertManager = new AlertManager();
            this.AlertManager.Alert += this.AlertManager_Alert;
        }

        private TimeSpan FetchInterval { get; } = TimeSpan.FromMilliseconds(500);
        private TimeSpan ReportDelay { get; } = TimeSpan.FromMilliseconds(1000);
        private TimeSpan ReportInterval { get; } = TimeSpan.FromSeconds(2);

        private DateTime NextReportTimeUtc { get; set; }
        private DateTime MinAcceptableTimeStampUtc { get; set; }

        private StatisticsProcessor GlobalStatistics { get; }
        private List<StatisticsProcessor> PeriodicStatistics { get; set; } = new List<StatisticsProcessor>();

        private LogEntryProvider LogEntryProvider { get; }

        private AlertManager AlertManager { get; }

        public void Dispose()
        {
            this.LogEntryProvider.Dispose();
            this.AlertManager.Alert -= this.AlertManager_Alert;
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
                this.AlertManager.Update(utcNow);

                //Console.WriteLine($"NowUtc: {utcNow.ToString("mm:ss.fff")}, NextReportTime: {this.NextReportTimeUtc.ToString("mm:ss.fff")}, {string.Join(", ", this.PeriodicStatistics.Select(s => $"{s.PeriodStartUtc.ToString("mm:ss.fff")}-{s.PeriodEndUtc.ToString("mm:ss.fff")}"))}");
            }
        }

        private void InitializeRun()
        {
            var utcNow = DateTime.UtcNow;
            this.UpdateNextReportTime(utcNow);

            this.AlertManager.Initialize(utcNow);
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

            this.AlertManager.Process(logEntry);
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
            return;

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

        private void AlertManager_Alert(object sender, AlertEventArgs e)
        {
            if(e.AlertEvent == AlertEvent.Raised)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"High traffic generated an alert - hits = {e.HitsCount}, triggered at {e.TimeStamp.ToLongTimeString()}");
                Console.ForegroundColor = color;
            }
            else if(e.AlertEvent == AlertEvent.Canceled)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Recovered from the altert - hits = {e.HitsCount}, triggered at {e.TimeStamp.ToLongTimeString()}");
                Console.ForegroundColor = color;
            }
        }
    }
}
