using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace LogGenerator
{
    public class Program
    {
        private const string LogPath = @"C:\Users\tom_c\Downloads\access-log-sample.log";
        private const int DelayMilliseconds = 100;

        public static void Main(string[] args)
        {
            using (var cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (_, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    cts.Cancel();
                };

                try
                {
                    using (var sinkStream = File.Open(LogPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    using (var writer = new StreamWriter(sinkStream, Encoding.ASCII))
                    {
                        Console.WriteLine($"Writing to \"{sinkStream.Name}\"");

                        while (true)
                        {
                            Console.Write(".");
                            cts.Token.ThrowIfCancellationRequested();

                            var logEntry = GenerateLogEntry();
                            writer.WriteLine(logEntry);
                            writer.Flush();

                            Thread.Sleep(DelayMilliseconds);
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadKey(true);
                }
            }
        }

        private static string GenerateLogEntry()
        {
            var clientAddress = "127.0.0.1";
            var userId = "-";
            var userName = "frank";
            var dateTime = DateTime.Now.ToString("dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture);
            var request = "GET /api/endpoint HTTP/1.0";
            var status = 200;
            var objectSize = 48;

            var colonIndex = dateTime.LastIndexOf(':');
            dateTime = dateTime.Substring(0, colonIndex) + dateTime.Substring(colonIndex + 1);

            FormattableString formattable = $"{clientAddress} {userId} {userName} [{dateTime}] \"{request}\" {status} {objectSize}";
            return formattable.ToString(CultureInfo.InvariantCulture);
        }
    }
}
