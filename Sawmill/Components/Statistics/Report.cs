namespace Sawmill.Components.Statistics
{
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
