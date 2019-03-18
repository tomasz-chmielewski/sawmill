namespace Sawmill.Components.Statistics.Collectors.Abstractions
{
    /// <summary>
    /// Represents a single URL section statistics.
    /// </summary>
    public interface IUrlSection
    {
        string SectionPath { get; }
        int HitCount { get; }
    }
}
