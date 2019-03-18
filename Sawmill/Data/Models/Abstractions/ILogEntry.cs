using System;
using System.Net;

namespace Sawmill.Data.Models.Abstractions
{
    /// <summary>
    /// Represents a www server log entry.
    /// </summary>
    public interface ILogEntry
    {
        IPAddress ClientAddress { get; }
        string UserId { get; }
        string UserName { get; }
        DateTime TimeStampUtc { get; }
        ILogEntryRequest Request { get; }
        int Status { get; }
        int? ObjectSize { get; }
    }
}
