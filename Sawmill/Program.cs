using Sawmill.Application;
using System;
using System.Threading;

namespace Sawmill
{
    public static class Program
    {
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
                    using (var appFactory = new SawmillApplicationFactory())
                    using (var app = appFactory.Create())
                    {
                        app.Run(cancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.ForegroundColor = color;
                }
            }
        }
    }
}
