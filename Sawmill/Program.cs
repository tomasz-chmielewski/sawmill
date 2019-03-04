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
        private const string SourceFilePath = @"C:\Users\tom_c\Downloads\access.log";

        private static int count = 0;
 
        public static void Main(string[] args)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var cancellationToken = cancellationTokenSource.Token;

                var feedSourceFileTask = RunFeedSourceFileTask(1024 * 1024, 100, cancellationToken);
                var processLogFileTask = Task.Run(() =>
                {
                    using (var reader = new LogStreamReader(File.Open(LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        while (true)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var line = reader.ReadLine();

                            if(line != null)
                            {
                                if (count % 10000 == 0)
                                {
                                    Console.WriteLine(" " + count);
                                }

                                count++;

                                // remove it ?
                                line = line.Replace("\\\"", "%22");

                                if(!LogEntry.TryParse(line, out LogEntry logEntry))
                                {
                                    Console.WriteLine("error: " + line);
                                }
                            }
                            else
                            {
                                Thread.Sleep(250);
                            }
                        }
                    }
                }, cancellationToken);

                Console.CancelKeyPress += (_, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    cancellationTokenSource.Cancel();
                };

                var tasks = new[] { feedSourceFileTask, processLogFileTask };

                try
                {
                    Task.WaitAny(tasks, cancellationToken);
                }
                catch(Exception e)
                {
                    Console.WriteLine("WaitAny: " + e.Message);
                    Console.ReadKey(true);
                }

                cancellationTokenSource.Cancel();
                try
                {
                    Task.WaitAll(tasks);
                }
                catch(Exception e)
                {
                    Console.WriteLine("WaitAll: " + e.Message);
                    Console.ReadKey(true);
                }
            }
        }

        private static Task RunFeedSourceFileTask(int bufferSize, int delayMilliseconds, CancellationToken cancellationToken)
        {
            var task = Task.Run(() =>
            {
                using (var sourceStream = File.Open(SourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var sinkStream = File.Open(LogPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    var buffer = new byte[bufferSize];

                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        //Console.WriteLine("Feeding the source file");

                        var bytesRead = sourceStream.Read(buffer, 0, buffer.Length);
                        sinkStream.Write(buffer, 0, bytesRead);
                        sinkStream.Flush();

                        Thread.Sleep(delayMilliseconds);
                    }
                }
            }, cancellationToken);

            Thread.Sleep(100);

            return task;
        }

        //public static async Task Main(string[] args)
        //{
        //    using (var cancellationTokenSource = new CancellationTokenSource())
        //    {
        //        var cancellationToken = cancellationTokenSource.Token;

        //        var feedSourceFileTask = FeedSourceFileAsync(cancellationToken);
        //        var processLogFileTask = ProcessLogFileAsync(cancellationToken);
        //        //var waintForUserInputTask = WaintForUserInputAsync(cancellationToken);

        //        var tasks = new[] { /*waintForUserInputTask,*/ feedSourceFileTask, processLogFileTask };

        //        ConsoleCancelEventHandler consoleCancelEventHandler = null;
        //        consoleCancelEventHandler = (_, eventArgs) =>
        //        {
        //            Console.CancelKeyPress -= consoleCancelEventHandler;
        //            eventArgs.Cancel = true;
        //            Task.Run(() => cancellationTokenSource.Cancel());
        //        };

        //        Console.CancelKeyPress += consoleCancelEventHandler;

        //        try
        //        {
        //            await Task.WhenAny(tasks);
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine($"WhenAny: {e.Message}");
        //        }

        //        cancellationTokenSource.Cancel();

        //        try
        //        {
        //            await Task.WhenAll(tasks);
        //        }
        //        //catch (OperationCanceledException)
        //        //{
        //        //    Console.WriteLine("canceled");
        //        //}
        //        catch (Exception e)
        //        {
        //            Console.WriteLine($"WhenAll: {e.Message}");
        //        }
        //    }

        //    Console.ReadKey(true);
        //}

        //private static async Task ProcessLogFileAsync(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        using (var reader = new StreamReader(File.Open(LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
        //        {
        //            var buffer = new char[1024];

        //            while (true)
        //            {
        //                cancellationToken.ThrowIfCancellationRequested();
        //                var line = await reader.ReadBlockAsync(buffer, cancellationToken);
        //                Console.Write(line);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"ProcessLogFileAsync: {e.Message}");
        //        throw;
        //    }
        //}

        //private static async Task FeedSourceFileAsync(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        using (var sourceStream = File.Open(SourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        //        using (var sinkStream = File.Open(LogPath, FileMode.Create, FileAccess.Write, FileShare.Read))
        //        {
        //            const int BufferSize = 1024;
        //            var buffer = new byte[BufferSize];

        //            //var feedStart = DateTime.UtcNow;
        //            //var delay = TimeSpan.FromSeconds(1.0);

        //            while (!cancellationToken.IsCancellationRequested)
        //            {
        //                Console.WriteLine("Feeding the source file");

        //                var bytesRead = await sourceStream.ReadAsync(buffer, 0, BufferSize, cancellationToken);
        //                await sinkStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
        //                await sinkStream.FlushAsync();

        //                //var feedEnd = DateTime.UtcNow;
        //                //var feedTimeSpan = feedEnd - feedStart;

        //                //if (feedTimeSpan < delay)
        //                //{
        //                //    await Task.Delay(delay - feedTimeSpan, cancellationToken);
        //                //}

        //                //feedStart = feedEnd;

        //                await Task.Delay(1000, cancellationToken);
        //            }
        //        }
        //    }
        //    catch(Exception e)
        //    {
        //        Console.WriteLine($"FeedSourceFileAsync: {e.Message}");
        //        throw;
        //    }
        //}

        //private static async Task WaintForUserInputAsync(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        while (!Console.KeyAvailable)
        //        {
        //            await Task.Delay(1000, cancellationToken);
        //        }

        //        Console.ReadKey(true);
        //        Console.WriteLine("User input");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"WaintForUserInputAsync: {e.Message}");
        //        throw;
        //    }
        //}
    }
}
