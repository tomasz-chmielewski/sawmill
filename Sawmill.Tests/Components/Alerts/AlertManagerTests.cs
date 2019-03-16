using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Sawmill.Common.DateAndTime.Extensions;
using Sawmill.Components.Alerts;
using Sawmill.Components.Alerts.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sawmill.Tests.Components.Alerts
{
    public class AlertManagerTests
    {
        /// <summary>
        /// A simulated startup time, set to 2019-03-09 20:00:00.200 UTC. It can be any DateTime constant.
        /// </summary>
        private DateTime StartUtc { get; } = new DateTime(2019, 3, 9, 20, 0, 0, 200, DateTimeKind.Utc);

        [TestCase(5, 10, 51, /*inflow:*/ 51)]                   // request peak: [51]
        [TestCase(5, 10, 51, /*inflow:*/ 0, 0, 0, 0, 0, 0, 51)] // request peak: 0, 0, [0, 0, 0, 0, 51]
        [TestCase(5, 10, 61, /*inflow:*/ 5, 5, 51)]             // request peak: [5, 5, 51]
        [TestCase(5, 10, 71, /*inflow:*/ 5, 5, 5, 5, 5, 5, 51)] // request peak: 5, 5, [5, 5, 5, 5, 51]
        [TestCase(5, 1, 6, /*inflow:*/ 6)]                      // request peak: [6]
        [TestCase(5, 1, 6, /*inflow:*/ 0, 0, 6)]                // request peak: [0, 0, 6]
        [TestCase(5, 1, 8, /*inflow:*/ 1, 1, 6)]                // request peak: [1, 1, 6]
        [TestCase(5, 1, 10, /*inflow:*/ 1, 1, 1, 1, 1, 1, 6)]   // request peak: 1, 1, [1, 1, 1, 1, 6]
        [TestCase(5, 1, 6, /*inflow:*/ 2, 2, 2)]                // constant inflow: [2, 2, 2]
        [TestCase(5, 10, 55, /*inflow:*/ 11, 11, 11, 11, 11)]   // constant inflow: [11, 11, 11, 11, 11]
        [TestCase(5, 1, 6, /*inflow:*/ 1, 2, 0, 1, 0, 2, 0, 0, 4)]  // random inflow: 1, 2, 0, 1, [0, 2, 0, 0, 4]
        [TestCase(5, 1, 7, /*inflow:*/ 1, 1, 1, 1, 1, 0, 1, 2, 3)]  // random inflow: 1, 1, 1, 1, [1, 0, 1, 2, 3]
        public void RaiseAlertWhenHitCountExceedsThreshold(int monitoredPeriodSeconds, int hitsPerSecondsThreshold, int expectedHitCount, params int[] inflow)
        {
            var alertHandler = new Mock<IAlertHandler>();
            var options = this.CreateOptions(monitoredPeriodSeconds, 0, hitsPerSecondsThreshold);
            var utcNow = this.StartUtc;

            var alertManager = new AlertManager(options, alertHandler.Object);

            // [0, 0, 0, 0, 0 | 0]
            alertManager.Initialize(utcNow);

            foreach (var requestsCount in inflow)
            {
                alertHandler.Verify(x => x.RaiseAlert(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never());

                // -> [n, n, n, n, n | requestsCount]
                alertManager.Process(this.CreateLogs(requestsCount, utcNow));
                alertManager.MoveMonitoredPeriod(utcNow);
                utcNow.IncreaseBySeconds(1);
            }

            // -> [n, n, n, n, n | 0]
            alertManager.Process(this.CreateLogs(0, utcNow));
            alertManager.MoveMonitoredPeriod(utcNow);
            alertHandler.Verify(x => x.RaiseAlert(utcNow.FloorSeconds(1), expectedHitCount), Times.Once());
        }

        [TestCase(5, 3, 10, /*inflow:*/ 51, 0, 0, 0)]                   // alert delayed: [51], 0, 0, 0
        [TestCase(5, 3, 10, /*inflow:*/ 0, 0, 0, 0, 0, 0, 51, 0, 0, 0)] // alert delayed: 0, 0, [0, 0, 0, 0, 51], 0, 0, 0
        public void RaiseAlertWhithDelay(int monitoredPeriodSeconds, int delaySeconds, int hitsPerSecondsThreshold, params int[] inflow)
        {
            DateTime? alertRepotTime = null;

            var alertHandler = new Mock<IAlertHandler>();
            alertHandler
                .Setup(x => x.RaiseAlert(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Callback((DateTime timeStamp, int hitCount) => alertRepotTime = timeStamp);

            var options = this.CreateOptions(monitoredPeriodSeconds, delaySeconds, hitsPerSecondsThreshold);
            var utcNow = this.StartUtc;

            var alertManager = new AlertManager(options, alertHandler.Object);

            // [0, 0, 0, 0, 0 | 0]
            alertManager.Initialize(utcNow);

            foreach (var requestsCount in inflow)
            {
                // -> [n, n, n, n, n | requestsCount]
                alertManager.Process(this.CreateLogs(requestsCount, utcNow));
                alertManager.MoveMonitoredPeriod(utcNow);
                utcNow.IncreaseBySeconds(1);
            }

            // -> [n, n, n, n, n | 0]
            alertManager.Process(this.CreateLogs(0, utcNow));
            alertManager.MoveMonitoredPeriod(utcNow);

            var expectedAlertReportTime = utcNow.FloorSeconds(1) - TimeSpanEx.FromSecondsInt(delaySeconds);
            alertHandler.Verify(x => x.RaiseAlert(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once());

            Assert.AreEqual(expectedAlertReportTime, alertRepotTime.Value);
        }

        [TestCase(5, 10, /*inflow:*/ 0, 0, 50, 0, 0, 0, 0, 0)]          // request peak
        [TestCase(5, 10, /*inflow:*/ 5, 5, 40, 0, 0, 0, 0, 0)]          // request peak
        [TestCase(5, 10, /*inflow:*/ 5, 5, 5, 5, 5, 30, 5, 5, 5, 5, 5)] // request peak
        [TestCase(5, 1, /*inflow:*/ 0, 0, 5, 0, 0, 0, 0, 0)]            // request peak
        [TestCase(5, 1, /*inflow:*/ 1, 1, 1, 1, 1, 1, 1, 1, 1)] // constant inflow
        [TestCase(5, 10, /*inflow:*/ 10, 10, 10, 10, 10, 10)]   // constant inflow
        [TestCase(5, 1, /*inflow:*/ 1, 2, 0, 1, 0, 2, 0, 0, 3)] // random inflow
        public void DontRaiseAlertIfHitCountNotExceedsThreshold(int monitoredPeriodSeconds, int hitsPerSecondsThreshold, params int[] inflow)
        {
            var alertHandler = new Mock<IAlertHandler>();
            var options = this.CreateOptions(monitoredPeriodSeconds, 0, hitsPerSecondsThreshold);
            var utcNow = this.StartUtc;

            var alertManager = new AlertManager(options, alertHandler.Object);

            // [0, 0, 0, 0, 0 | 0]
            alertManager.Initialize(utcNow);

            foreach (var requestsCount in inflow)
            {
                alertHandler.Verify(x => x.RaiseAlert(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never());

                // -> [n, n, n, n, n | requestsCount]
                alertManager.Process(this.CreateLogs(requestsCount, utcNow));
                alertManager.MoveMonitoredPeriod(utcNow);
                utcNow.IncreaseBySeconds(1);
            }

            // -> [n, n, n, n, n | 0]
            alertManager.Process(this.CreateLogs(0, utcNow));
            alertManager.MoveMonitoredPeriod(utcNow);
            alertHandler.Verify(x => x.RaiseAlert(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never());
        }

        [TestCase(5, 10, 0, /*inflow:*/ 51, 0, 0, 0, 0, 0)]                 // request peak: 51, [0, 0, 0, 0, 0]
        [TestCase(5, 10, 48, /*inflow:*/ 12, 12, 12, 12, 12, 12, 0)]        // request peak: 12, 12, [12, 12, 12, 12, 0]
        [TestCase(5, 10, 50, /*inflow:*/ 12, 12, 12, 12, 12, 14, 0)]        // request peak: 12, 12, [12, 12, 12, 14, 0]
        [TestCase(5, 10, 46, /*inflow:*/ 10, 11, 10, 11, 10, 11, 10, 4)]    // request peak: 10, 11, 10, [11, 10, 11, 10, 4]
        [TestCase(5, 1, 5, /*inflow:*/ 2, 2, 2, 1, 1, 1, 1, 1)]                     // constant inflow: 2, 2, 2, [1, 1, 1, 1, 1]
        [TestCase(5, 1, 4, /*inflow:*/ 2, 2, 2, 0, 0, 0)]                           // constant inflow: 2, [2, 2, 0, 0, 0]
        [TestCase(5, 10, 50, /*inflow:*/ 11, 11, 11, 11, 11, 10, 10, 10, 10, 10)]   // constant inflow: 11, 11, 11, 11, 11, [10, 10, 10, 10, 10]
        [TestCase(5, 10, 49, /*inflow:*/ 11, 11, 11, 11, 11, 5)]                    // constant inflow: 11, [11, 11, 11, 11, 5]
        [TestCase(5, 10, 25, /*inflow:*/ 50, 50, 50, 50, 50, 5, 5, 5, 5, 5)]        // constant inflow: 50, 50, 50, 50, 50, [5, 5, 5, 5, 5]
        [TestCase(5, 1, 5, /*inflow:*/ 1, 2, 3, 0, 1, 2, 0, 2)]                 // random inflow: 1, 2, 3, [0, 1, 2, 0, 2]
        [TestCase(5, 10, 30, /*inflow:*/ 12, 43, 30, 2, 40, 13, 7, 8, 0, 2)]    // random inflow: 12, 43, 30, 2, 40, [13, 7, 8, 0, 2]
        public void RecoverFromAlertWhenHitCountDropsBelowThreshold(int monitoredPeriodSeconds, int hitsPerSecondsThreshold, int expectedHitCount, params int[] inflow)
        {
            var alertHandler = new Mock<IAlertHandler>();
            var options = this.CreateOptions(monitoredPeriodSeconds, 0, hitsPerSecondsThreshold);
            var utcNow = this.StartUtc;

            var alertManager = new AlertManager(options, alertHandler.Object);

            // [0, 0, 0, 0, 0 | 0]
            alertManager.Initialize(utcNow);

            foreach (var requestsCount in inflow)
            {
                alertHandler.Verify(x => x.RecoverFromAlert(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never());

                // -> [n, n, n, n, n | requestsCount]
                alertManager.Process(this.CreateLogs(requestsCount, utcNow));
                alertManager.MoveMonitoredPeriod(utcNow);
                utcNow.IncreaseBySeconds(1);
            }

            // -> [n, n, n, n, n | 0]
            alertManager.Process(this.CreateLogs(0, utcNow));
            alertManager.MoveMonitoredPeriod(utcNow);
            alertHandler.Verify(x => x.RecoverFromAlert(utcNow.FloorSeconds(1), expectedHitCount), Times.Once());
        }

        [Test]
        public void DontAcceptOldRequests()
        {
            var alertHandler = new Mock<IAlertHandler>();
            var options = this.CreateOptions(5, 0, 10);
            var utcNow = this.StartUtc;

            var alertManager = new AlertManager(options, alertHandler.Object);

            alertManager.Initialize(utcNow);
            var processedRequests = alertManager.Process(this.CreateLogs(5, utcNow.AddSecondsInt(-1)));

            Assert.AreEqual(0, processedRequests);
        }

        [Test]
        public void CannotMoveBackInTime()
        {
            var alertHandler = new Mock<IAlertHandler>();
            var options = this.CreateOptions(5, 0, 10);
            var utcNow = this.StartUtc;

            var alertManager = new AlertManager(options, alertHandler.Object);

            alertManager.Initialize(utcNow);
            alertManager.Process(this.CreateLogs(5, utcNow));
            alertManager.MoveMonitoredPeriod(utcNow);

            Assert.Throws<InvalidOperationException>(() => alertManager.MoveMonitoredPeriod(utcNow.AddSecondsInt(-1)));
        }

        private IOptions<AlertManagerOptions> CreateOptions(int monitoredPeriodSeconds, int delaySeconds, int hitsPerSecondsThreshold)
        {
            var mock = new Mock<IOptions<AlertManagerOptions>>();
            mock.SetupGet(o => o.Value).Returns(new AlertManagerOptions
            {
                DelaySeconds = delaySeconds,
                HitsPerSecondsThreshold = hitsPerSecondsThreshold,
                MonitoredPeriodSeconds = monitoredPeriodSeconds
            });

            return mock.Object;
        }

        private ILogEntry CreateLog(DateTime timeStampUtc)
        {
            var mock = new Mock<ILogEntry>();
            mock.SetupGet(l => l.TimeStampUtc).Returns(timeStampUtc);
            return mock.Object;
        }

        private IEnumerable<ILogEntry> CreateLogs(int count, DateTime timeStampUtc)
        {
            return Enumerable.Range(0, count).Select(_ => this.CreateLog(timeStampUtc));
        }
    }
}