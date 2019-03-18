using Sawmill.Data.Models.Abstractions;
using System;

namespace Sawmill.Data.Models
{
    /// <summary>
    /// Represents the request part of a www server log entry.
    /// </summary>
    public class LogEntryRequest : ILogEntryRequest
    {
        public string Method { get; set; }
        public Uri Uri { get; set; }
        public string Protocol { get; set; }
    }
}
