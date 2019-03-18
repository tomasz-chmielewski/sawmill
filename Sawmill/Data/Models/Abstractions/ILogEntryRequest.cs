using System;

namespace Sawmill.Data.Models.Abstractions
{
    /// <summary>
    /// Represents the request part of a www server log entry.
    /// </summary>
    public interface ILogEntryRequest
    {
        string Method { get; }
        Uri Uri { get; }
        string Protocol { get; }
    }
}
