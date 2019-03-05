using Sawmill.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sawmill
{
    public class Program
    {
        private const string LogPath = @"C:\Users\tom_c\Downloads\access-log-sample.log";

        public static void Main(string[] args)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (_, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    cancellationTokenSource.Cancel();
                };

                try
                {
                    ProcessLogFile(cancellationTokenSource.Token);
                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadKey(true);
                }
            }
        }

        private static void ProcessLogFile(CancellationToken cancellationToken)
        {
            using (var fileStream = File.Open(LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new LogStreamReader(fileStream))
            {
                Console.WriteLine($"Reading the log file: \"{fileStream.Name}\"");

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var line = reader.ReadLine();

                    if (line != null)
                    {
                        Console.Write('.');

                        // remove it ?
                        //line = line.Replace("\\\"", "%22");

                        if (!LogEntry.TryParse(line, out LogEntry logEntry))
                        {
                            Console.WriteLine("\nERROR: " + line);
                        }
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
        }
    }
}
