using Moq;
using NUnit.Framework;
using Sawmill.Components.Alerts;
using Sawmill.Components.Alerts.Abstractions;
using Sawmill.Models;
using System;
using System.Linq;

namespace Sawmill.Tests.Components.Alerts
{
    public class AlertManagerTests
    {
        [Test]
        public void RaiseAlertWhenThreasholdExceeded()
        {
            var alertHandler = new Mock<IAlertHandler>();

            var alertManager = new AlertManager(alertHandler.Object);

            var startUtc = new DateTime(2019, 3, 9, 20, 0, 0, DateTimeKind.Utc);

            LogEntry.TryParse("127.0.0.1 - james [09/Mar/2019:20:00:00 +0000] \"GET / report HTTP / 1.0\" 200 123", out var logEntry);

            alertManager.Initialize(startUtc);
            alertManager.Process(startUtc, Enumerable.Range(0, 51).Select(_ => logEntry));
            alertManager.Process(startUtc.AddSeconds(1), Enumerable.Empty<LogEntry>());

            alertHandler.Verify(x => x.RaiseAlert(startUtc.AddSeconds(1), 51));
        }
    }
}