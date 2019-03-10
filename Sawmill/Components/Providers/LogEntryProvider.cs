using Microsoft.Extensions.Options;
using Sawmill.Common.IO;
using Sawmill.Components.Providers.Abstractions;
using Sawmill.Data;
using Sawmill.Data.Models;
using Sawmill.Data.Models.Abstractions;
using System;
using System.IO;

namespace Sawmill.Components.Providers
{
    public sealed class LogEntryProvider : ILogEntryProvider
    {
        public LogEntryProvider(IOptions<LogEntryProviderOptions> optionsAccessor)
        {
            var options = optionsAccessor.Value;
            this.Path = options.Path;
        }

        public string Path { get; }

        private LineReader Reader { get; set; }
        private LogEntrySerializer Serializer { get; } = new LogEntrySerializer();

        public void Dispose()
        {
            this.Close();
        }

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
                this.Reader = new LineReader(fileStream);
            }
            catch
            {
                fileStream?.Dispose();
                throw;
            }
        }

        public void Close()
        {
            if (this.Reader != null)
            {
                this.Reader.Dispose();
                this.Reader = null;
            }
        }

        public ILogEntry GetEntry()
        {
            var line = this.Reader.ReadLine();
            if (line == null)
            {
                return null;
            }

            return Serializer.TryParse(line, out LogEntry logEntry)
                ? logEntry 
                : null;
        }
    }
}
