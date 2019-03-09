using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Sawmill.Components.Alerts;
using Sawmill.Components.Alerts.Abstractions;
using Sawmill.Models;
using Sawmill.Models.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sawmill.Tests.Components.Alerts
{
    public class AlertManagerAction
    {
        public AlertManagerAction(DateTime dateTime, params DateTime[] requestTimeStamps)
        {
            this.DateTime = dateTime;
            this.RequestTimeStamps = requestTimeStamps;
        }

        public DateTime DateTime { get; set; }
        public IEnumerable<DateTime> RequestTimeStamps { get; set; }

        public void Perform(IAlertManager alertManager)
        {
            var requestMocks = RequestTimeStamps.Select(t =>
            {
                var mock = new Mock<ILogEntry>();
                mock.SetupGet(m => m.TimeStampUtc).Returns(t);
                return mock;
            });

            alertManager.Process(this.DateTime, requestMocks.Select(m => m.Object));
        }
    }

    public class AlertManagerTests
    {
        [Test]
        public void RaiseAlertWhenThreasholdExceeded()
        {
            var alertHandler = new Mock<IAlertHandler>();
            var options = new Mock<IOptions<AlertManagerOptions>>();
            options.SetupGet(o => o.Value).Returns(new AlertManagerOptions
            {
                DelaySeconds = 1,
                HitsPerSecondsThreshold = 1,
                MonitoredPeriodSeconds = 10
            });

            var logEntry = new Mock<ILogEntry>();
            logEntry.SetupGet(l => l.TimeStampUtc).Returns(new DateTime(2019, 3, 9, 20, 0, 0, DateTimeKind.Utc));

            var alertManager = new AlertManager(options.Object, alertHandler.Object);

            var startUtc = new DateTime(2019, 3, 9, 20, 0, 0, DateTimeKind.Utc);

            //LogEntry.TryParse("127.0.0.1 - james [09/Mar/2019:20:00:00 +0000] \"GET / report HTTP / 1.0\" 200 123", out var logEntry);

            alertManager.Initialize(startUtc);
            alertManager.Process(startUtc, Enumerable.Range(0, 51).Select(_ => logEntry.Object));
            alertManager.Process(startUtc.AddSeconds(1), Enumerable.Empty<LogEntry>());

            alertHandler.Verify(x => x.RaiseAlert(startUtc.AddSeconds(1), 51));
        }

        private static IOptions<AlertManagerOptions> CreateOptions(int monitoredPeriodSeconds, int delaySeconds, int hitsPerSecondsThreshold)
        {
            var options = new Mock<IOptions<AlertManagerOptions>>();
            options.SetupGet(o => o.Value).Returns(new AlertManagerOptions
            {
                DelaySeconds = 1,
                HitsPerSecondsThreshold = 1,
                MonitoredPeriodSeconds = 10
            });

            return options.Object;
        }

        public void Test()
        {
            var alertHandler = new Mock<IAlertHandler>();
            var options = CreateOptions(10, 1, 1);

            var logEntry = new Mock<ILogEntry>();
            logEntry.SetupGet(l => l.TimeStampUtc).Returns(new DateTime(2019, 3, 9, 20, 0, 0, DateTimeKind.Utc));

            var alertManager = new AlertManager(options, alertHandler.Object);

            var startUtc = new DateTime(2019, 3, 9, 20, 0, 0, DateTimeKind.Utc);

            //LogEntry.TryParse("127.0.0.1 - james [09/Mar/2019:20:00:00 +0000] \"GET / report HTTP / 1.0\" 200 123", out var logEntry);

            alertManager.Initialize(startUtc);


            var testScenario = new[]
            {
                new AlertManagerAction(new DateTime(), new [] { new DateTime() })
            };

            foreach(var action in testScenario)
            {
                action.Perform(alertManager);
            }
        }
    }
}