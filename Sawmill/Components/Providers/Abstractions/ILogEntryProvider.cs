using Sawmill.Data.Models.Abstractions;
using System;

namespace Sawmill.Components.Providers.Abstractions
{
    /// <summary>
    /// Represents object that provides instances of <see cref="ILogEntry"/>.
    /// </summary>
    public interface ILogEntryProvider : IDisposable
    {
        /// <summary>
        /// Path of the input file.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Opens a stream on the specified path.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the current stream and releases any resources.
        /// </summary>
        void Close();

        /// <summary>
        /// Returns the next available log entry.
        /// </summary>
        /// <returns>Next available log entry or null if there are no entries available at the time.</returns>
        ILogEntry GetEntry();
    }
}
