using System;

namespace Sawmill.Common.Extensions
{
    public static class DateAndTimeExtensions
    {
        public const long TicksPerSecond = 10_000_000;
        public const long TicksPerMilliseconds = 10_000;

        public static DateTime Floor(this DateTime dateTime, long baseTicks)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % baseTicks), dateTime.Kind);
        }

        public static DateTime Floor(this DateTime dateTime, TimeSpan baseTimeSpan)
        {
            return dateTime.Floor(baseTimeSpan.Ticks);
        }

        public static DateTime FloorSeconds(this DateTime dateTime, int seconds)
        {
            return dateTime.Floor(seconds * TicksPerSecond);
        }

        public static DateTime Ceiling(this DateTime dateTime, long baseTicks)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % baseTicks) + baseTicks, dateTime.Kind);
        }

        public static DateTime Ceiling(this DateTime dateTime, TimeSpan baseTimeSpan)
        {
            return dateTime.Ceiling(baseTimeSpan.Ticks);
        }

        public static DateTime CeilingSeconds(this DateTime dateTime, int seconds)
        {
            return dateTime.Ceiling(seconds * TicksPerSecond);
        }

        public static int TotalMillisecondsAsInt(this TimeSpan timeSpan)
        {
            return (int)(timeSpan.Ticks / TicksPerMilliseconds);
        }

        public static int TotalSecondsAsInt(this TimeSpan timeSpan)
        {
            return (int)(timeSpan.Ticks / TicksPerSecond);
        }

        public static DateTime AddSecondsInt(this DateTime dateTime, int seconds)
        {
            return dateTime.AddTicks(seconds * TicksPerSecond);
        }

        public static DateTime IncreaseBy(this ref DateTime dateTime, TimeSpan timeSpan)
        {
            dateTime = dateTime.Add(timeSpan);
            return dateTime;
        }

        public static DateTime IncreaseBySeconds(this ref DateTime dateTime, int seconds)
        {
            return dateTime.IncreaseBy(TimeSpanEx.FromSecondsInt(seconds));
        }
    }
}
