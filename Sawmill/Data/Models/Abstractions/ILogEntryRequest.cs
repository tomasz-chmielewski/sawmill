using System;

namespace Sawmill.Data.Models.Abstractions
{
    public interface ILogEntryRequest
    {
        string Method { get; }
        Uri Uri { get; }
        string Protocol { get; }
    }
}
