using System;

namespace Sawmill.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime Floor(this DateTime dateTime, long baseTicks)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % baseTicks), dateTime.Kind);
        }

        public static DateTime Floor(this DateTime dateTime, TimeSpan baseTimeSpan)
        {
            return dateTime.Floor(baseTimeSpan.Ticks);
        }

        public static DateTime Ceiling(this DateTime dateTime, long baseTicks)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % baseTicks) + baseTicks, dateTime.Kind);
        }

        public static DateTime Ceiling(this DateTime dateTime, TimeSpan baseTimeSpan)
        {
            return dateTime.Ceiling(baseTimeSpan.Ticks);
        }

        public static int TotalMillisecondsAsInt(this TimeSpan timeSpan)
        {
            return (int)(timeSpan.Ticks / 10_000);
        }
    }
}
