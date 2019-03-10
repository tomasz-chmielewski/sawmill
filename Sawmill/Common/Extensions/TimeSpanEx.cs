using System;

namespace Sawmill.Common.Extensions
{
    public static class TimeSpanEx
    {
        public static TimeSpan FromSecondsInt(int seconds)
        {
            return TimeSpan.FromTicks(DateAndTimeExtensions.TicksPerSecond * seconds);
        }

        public static TimeSpan FromMillisecondsInt(int milliseconds)
        {
            return TimeSpan.FromTicks(DateAndTimeExtensions.TicksPerMilliseconds * milliseconds);
        }
    }
}
