using Sawmill.Application;
using System;
using System.Threading;

namespace Sawmill
{
    public class Program
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
                    using (var application = new SawmillApplication())
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
