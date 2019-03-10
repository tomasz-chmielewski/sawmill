namespace Sawmill.Components.Statistics.Collectors.Abstractions
{
    public interface IUrlSection
    {
        string Path { get; }
        int HitCount { get; }
    }
}
