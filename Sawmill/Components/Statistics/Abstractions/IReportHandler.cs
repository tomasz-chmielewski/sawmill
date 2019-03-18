namespace Sawmill.Components.Statistics.Abstractions
{
    /// <summary>
    /// Represents oan bject that handles statistics report related events.
    /// </summary>
    public interface IReportHandler
    {
        void HandleReport(Report report);
    }
}
