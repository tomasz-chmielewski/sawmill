using Sawmill.Components.Alerts.Abstractions;
using System;

namespace Sawmill.Components.Alerts
{
    public class AlertHandler : IAlertHandler
    {
        public void RaiseAlert(DateTime timeStamp, int hitCount)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"High traffic generated an alert - hits = {hitCount}, triggered at {timeStamp.ToLongTimeString()}");
            Console.ForegroundColor = color;
        }

        public void RecoverFromAlert(DateTime timeStamp, int hitCount)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Recovered from the altert - hits = {hitCount}, triggered at {timeStamp.ToLongTimeString()}");
            Console.ForegroundColor = color;
        }
    }
}
