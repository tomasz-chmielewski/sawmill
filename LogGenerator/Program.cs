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
        private const string SourcePath = @"C:\Users\tom_c\Downloads\access.log";

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
                    //using (var sourceStream = File.Open(SourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sinkStream = File.Open(LogPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                    using (var writer = new StreamWriter(sinkStream, Encoding.ASCII))
                    {
                        sinkStream.Seek(0, SeekOrigin.End);

                        //Console.WriteLine($"Reading from \"{sourceStream.Name}\"");
                        Console.WriteLine($"Writing to \"{sinkStream.Name}\"");

                        //var buffer = new byte[1024];

                        while (true)
                        {
                            Console.Write(".");
                            cts.Token.ThrowIfCancellationRequested();

                            //var bytesRead = sourceStream.Read(buffer);
                            //sinkStream.Write(buffer, 0, bytesRead);

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
            var random = Random.Next(100000);
            var isInvalidLine = random == 0;
            var isLineTooLong = random == 1;
            var isOutdatedLog = random == 2;

            if (isInvalidLine)
            {
                return "127.0.0.1 - frank [15/MarMar/2019:21:44:36 +0100] \"GET / api / user / 12 / posts ? q = abc HTTP / 1.0\" 411 48";
            }

            if(isLineTooLong)
            {
                return new string('x', 20000);
            }

            var endpoint = ValidEndpoints[Random.Next(ValidEndpoints.Length)];

            var timeStamp = !isOutdatedLog ? DateTime.Now : (DateTime.Now - new TimeSpan(0, 0, 10));

            var clientAddress = "127.0.0.1";
            var userId = "-";
            var userName = "frank";
            var dateTime = timeStamp.ToString("dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture);
            var request = $"GET {endpoint} HTTP/1.0";
            var status = Random.Next(200, 599);
            var objectSize = 48;


            var colonIndex = dateTime.LastIndexOf(':');
            dateTime = dateTime.Substring(0, colonIndex) + dateTime.Substring(colonIndex + 1);

            return FormattableString.Invariant($"{clientAddress} {userId} {userName} [{dateTime}] \"{request}\" {status} {objectSize}");
        }
    }
}
