using Microsoft.Extensions.Options;
using Sawmill.Common.Console;
using Sawmill.Common.IO;
using Sawmill.Components.Providers.Abstractions;
using Sawmill.Data;
using Sawmill.Data.Models.Abstractions;
using System;
using System.IO;

namespace Sawmill.Components.Providers
{
    /// <summary>
    /// Represents object that provides instances of <see cref="ILogEntry"/>.
    /// </summary>
    public sealed class LogEntryProvider : ILogEntryProvider
    {
        /// <summary>
        /// Creates a new instance of <see cref="LogEntryProvider"/>.
        /// </summary>
        /// <param name="optionsAccessor">Object configuration accessor.</param>
        public LogEntryProvider(IOptions<LogEntryProviderOptions> optionsAccessor)
        {
            var options = optionsAccessor.Value;
            this.Path = options.Path;
            this.MaxLineLength = options.MaxLineLength;
        }

        /// <summary>
        /// Path of the input file.
        /// </summary>
        public string Path { get; }

        private int MaxLineLength { get; }

        private LineReader Reader { get; set; }
        private LogEntrySerializer Serializer { get; } = new LogEntrySerializer();

        /// <summary>
        /// Releases all resources used by the object.
        /// </summary>
        public void Dispose()
        {
            this.Close();
        }

        /// <summary>
        /// Opens a stream on the specified path.
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.IO.PathTooLongException"></exception>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="System.UnauthorizedAccessException"></exception>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public void Open()
        {
            if (this.Reader != null)
            {
                return;
            }

            FileStream fileStream = null;
            try
            {
                fileStream = File.Open(this.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fileStream.Seek(0, SeekOrigin.End);

                this.Reader = new LineReader(fileStream, this.MaxLineLength);
            }
            catch
            {
                fileStream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Closes the current stream and releases any resources.
        /// </summary>
        public void Close()
        {
            if (this.Reader != null)
            {
                this.Reader.Dispose();
                this.Reader = null;
            }
        }

        /// <summary>
        /// Returns the next available log entry.
        /// </summary>
        /// <returns>Next available log entry or null if there are no entries available at the time.</returns>
        public ILogEntry GetEntry()
        {
            while (true)
            {
                string line;
                try
                {
                    line = this.Reader.ReadLine();
                    if (line == null)
                    {
                        return null;
                    }
                }
                catch (InvalidDataException e)
                {
                    this.HandleWarning(e.Message);
                    continue;
                }

                if(!this.Serializer.TryParse(line, out var logEntry))
                {
                    this.HandleWarning("Persing error");
                    continue;
                }

                return logEntry;
            }
        }

        private void HandleWarning(string message)
        {
            ConsoleEx.ColorWrite(ConsoleColor.Yellow, "Warning: ");
            ConsoleEx.WriteLine(message);
        }
    }
}
