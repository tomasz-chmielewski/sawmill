using Sawmill.Common.Console;
using Sawmill.Components.Alerts.Abstractions;
using System;

namespace Sawmill.Components.Alerts
{
    public class AlertHandler : IAlertHandler
    {
        public void RaiseAlert(DateTime timeStamp, int hitCount)
        {
            var message = $"High traffic generated an alert - hits = {hitCount}, triggered at {this.Format(timeStamp)}";

            this.Print(ConsoleColor.Red, message);
        }

        public void RecoverFromAlert(DateTime timeStamp, int hitCount)
        {
            var message = $"Recovered from the altert - hits = {hitCount}, triggered at {this.Format(timeStamp)}";

            this.Print(ConsoleColor.Green, message);
        }

        private string Format(DateTime value)
        {
            return value.ToLocalTime().ToLongTimeString();
        }

        private void Print(ConsoleColor color, string value)
        {
            ConsoleEx.ColorWriteLine(color, value);
        }
    }
}
