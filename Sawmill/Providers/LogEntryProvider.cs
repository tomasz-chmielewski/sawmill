using Sawmill.Common.IO;
using Sawmill.Models;
using Sawmill.Providers.Abstractions;
using System;
using System.IO;

namespace Sawmill.Providers
{
    public class LogEntryProvider : IDisposable, ILogEntryProvider
    {
        private const string LogPath = @"C:\Users\tom_c\Downloads\access-log-sample.log";

        private LogStreamReader Reader { get; set; }

        public void Dispose()
        {
            if(this.Reader != null)
            {
                this.Reader.Dispose();
                this.Reader = null;
            }
        }

        public LogEntry GetEntry()
        {
            this.EnsureReaderCreated();

            var line = this.Reader.ReadLine();
            if (line == null)
            {
                return null;
            }

            // remove it ?
            //line = line.Replace("\\\"", "%22");

            return LogEntry.TryParse(line, out LogEntry logEntry) 
                ? logEntry 
                : null;
        }

        private void EnsureReaderCreated()
        {
            if(this.Reader != null)
            {
                return;
            }

            var fileStream = File.Open(LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            try
            {
                this.Reader = new LogStreamReader(fileStream);
                Console.WriteLine($"Reading the log file: \"{fileStream.Name}\"");
            }
            catch
            {
                fileStream.Dispose();
                throw;
            }
        }
    }
}
