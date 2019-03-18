using Sawmill.Data.Models.Abstractions;
using System;
using System.Net;

namespace Sawmill.Data.Models
{
    /// <summary>
    /// Represents a www server log entry.
    /// </summary>
    public class LogEntry : ILogEntry
    {
        public IPAddress ClientAddress { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime TimeStampUtc { get; set; }
        public LogEntryRequest Request { get; set; }
        public int Status { get; set; }
        public int? ObjectSize { get; set; }

        ILogEntryRequest ILogEntry.Request => this.Request;
    }
}
