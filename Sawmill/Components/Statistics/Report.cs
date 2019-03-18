namespace Sawmill.Components.Statistics
{
    /// <summary>
    /// Represents the statistics report.
    /// </summary>
    public class Report
    {
        public Report(ReportItem global, ReportItem periodic)
        {
            this.Global = global;
            this.Periodic = periodic;
        }

        public ReportItem Global { get; }
        public ReportItem Periodic { get; }
    }
}
