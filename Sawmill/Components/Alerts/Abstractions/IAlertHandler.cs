using System;

namespace Sawmill.Components.Alerts.Abstractions
{
    public interface IAlertHandler
    {
        void RaiseAlert(DateTime timeStamp, int hitCount);
        void RecoverFromAlert(DateTime timeStamp, int hitCount);
    }
}
