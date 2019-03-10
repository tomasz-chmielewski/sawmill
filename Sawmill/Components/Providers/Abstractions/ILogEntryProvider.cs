using Sawmill.Data.Models.Abstractions;
using System;

namespace Sawmill.Components.Providers.Abstractions
{
    public interface ILogEntryProvider : IDisposable
    {
        string Path { get; }

        void Open();
        void Close();
        ILogEntry GetEntry();
    }
}
