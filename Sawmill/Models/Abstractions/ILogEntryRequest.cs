using System;

namespace Sawmill.Models.Abstractions
{
    public interface ILogEntryRequest
    {
        string Method { get; }
        Uri Uri { get; }
        string Protocol { get; }
    }
}
