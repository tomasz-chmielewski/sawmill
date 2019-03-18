using System;

namespace Sawmill.Components.Alerts.Abstractions
{
    /// <summary>
    /// Represents object that handles alert-related events.
    /// </summary>
    public interface IAlertHandler
    {
        void RaiseAlert(DateTime timeStamp, int hitCount);
        void RecoverFromAlert(DateTime timeStamp, int hitCount);
    }
}
