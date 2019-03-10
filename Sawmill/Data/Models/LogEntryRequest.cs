using Sawmill.Data.Models.Abstractions;
using System;

namespace Sawmill.Data.Models
{
    public class LogEntryRequest : ILogEntryRequest
    {
        public string Method { get; set; }
        public Uri Uri { get; set; }
        public string Protocol { get; set; }
    }
}
