using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace LogGenerator
{
    public class Program
    {
        private const string LogPath = @"C:\tmp\access.log";
        private const int DelayMilliseconds = 1;

        private static readonly Random Random = new Random();

        private static readonly string[] ValidEndpoints = new[]
        {
            "/",
            "/api",
            "/api/",
            "/api/items",
            "/api/posts",
            "/api/user/12/posts",
            "/api/user/12/posts?q=abc",
            "/data",
            "/users/test",
            "/endpoint1/test",
            "/endpoint2/test",
        };

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
                    using (var sinkStream = File.Open(LogPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                    using (var writer = new StreamWriter(sinkStream, Encoding.ASCII))
                    {
                        sinkStream.Seek(0, SeekOrigin.End);

                        Console.WriteLine($"Writing to \"{sinkStream.Name}\"");

                        while (true)
                        {
                            Console.Write(".");
                            cts.Token.ThrowIfCancellationRequested();

                            var logEntry = GenerateLogEntry();
                            writer.WriteLine(logEntry);
                            //writer.Flush();

                            //Thread.Sleep(DelayMilliseconds);
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
            var endpoint = ValidEndpoints[Random.Next(ValidEndpoints.Length)];

            var clientAddress = "127.0.0.1";
            var userId = "-";
            var userName = "frank";
            var dateTime = DateTime.Now.ToString("dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture);
            var request = $"GET {endpoint} HTTP/1.0";
            var status = Random.Next(200, 599);
            var objectSize = 48;

            var colonIndex = dateTime.LastIndexOf(':');
            dateTime = dateTime.Substring(0, colonIndex) + dateTime.Substring(colonIndex + 1);

            return FormattableString.Invariant($"{clientAddress} {userId} {userName} [{dateTime}] \"{request}\" {status} {objectSize}");
        }
    }
}
