using Sawmill.Application;
using Sawmill.Components.Alerts;
using Sawmill.Components.Statistics;
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
                    using (var application = new SawmillApplication(new AlertManager(new AlertHandler()), new StatisticsManager(new ReportHandler(), new StatisticsCollectionFactory())))
                    {
                        application.Run(cancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.ReadKey(true);
                }
            }
        }
    }
}
