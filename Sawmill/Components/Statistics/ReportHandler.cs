using Microsoft.Extensions.Options;
using Sawmill.Common.Console;
using Sawmill.Components.Statistics.Abstractions;
using Sawmill.Components.Statistics.Collectors;
using System;
using System.Globalization;
using System.Linq;

namespace Sawmill.Components.Statistics
{
    public class ReportHandler : IReportHandler
    {
        public ReportHandler(IOptions<ReportHandlerOptions> optionsAccessor)
        {
            var options = optionsAccessor.Value;
            this.MaxSectionCount = options.MaxUrlSectionCount;
        }

        private int MaxSectionCount { get; }

        private CultureInfo CultureInfo => CultureInfo.CurrentUICulture;

        public void HandleReport(Report report)
        {
            this.Report(report.Global);
            this.Report(report.Periodic);
        }

        private void Report(ReportItem report)
        {
            var statistics = report.Statistics;

            this.NewLine();
            this.PrintHeader(report.PeriodStart, report.PeriodEnd);

            this.PrintTotalHits(statistics.Hits);

            this.PrintSeparator();
            this.PrintStatusCodes(statistics.StatusCodes);

            this.PrintSeparator();
            this.PrintUrlSections(statistics.UrlSections);

            this.NewLine();
        }

        private void PrintHeader(DateTime periodStart, DateTime? periodEnd)
        {
            var startString = periodStart.ToLocalTime().ToLongTimeString();
            var endString = periodEnd != null
                ? periodEnd.Value.ToLocalTime().ToLongTimeString()
                : new string(' ', startString.Length);

            this.Print($"[{startString}-{endString}] ");
        }

        private void PrintTotalHits(TotalHits totalHits)
        {
            this.PrintCollectorHeader("hits");
            this.Print(totalHits.Count.ToString(this.CultureInfo));
        }

        private void PrintStatusCodes(StatusCodes statusCodes)
        {
            this.PrintCollectorHeader("2xx");
            this.Print(statusCodes.Hits2xx.ToString(this.CultureInfo));
            this.PrintSeparator();

            this.PrintCollectorHeader("3xx");
            this.Print(statusCodes.Hits3xx.ToString(this.CultureInfo));
            this.PrintSeparator();

            this.PrintCollectorHeader("4xx");
            this.Print(statusCodes.Hits4xx.ToString(this.CultureInfo));
            this.PrintSeparator();

            this.PrintCollectorHeader("5xx");
            this.Print(statusCodes.Hits5xx.ToString(this.CultureInfo));
        }

        private void PrintUrlSections(UrlSections urlSections)
        {
            this.PrintCollectorHeader("sections");

            var isFirstSection = true;

            this.Print("[");
            foreach (var section in urlSections.Take(this.MaxSectionCount))
            {
                if (!isFirstSection)
                {
                    this.PrintSeparator();
                }

                this.Print(ConsoleColor.White, section.Path);
                this.Print(": ");
                this.Print(section.HitCount.ToString(this.CultureInfo));

                isFirstSection = false;
            }

            this.Print("]");
        }

        private void PrintCollectorHeader(string header)
        {
            this.Print(ConsoleColor.Yellow, header);
            this.Print(": ");
        }

        private void PrintSeparator()
        {
            this.Print(", ");
        }

        private void Print(string value)
        {
            ConsoleEx.Write(value);
        }

        private void Print(ConsoleColor color, string value)
        {
            ConsoleEx.ColorWrite(color, value);
        }

        private void NewLine()
        {
            ConsoleEx.NewLine();
        }
    }
}
