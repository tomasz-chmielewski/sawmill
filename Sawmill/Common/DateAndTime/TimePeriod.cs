using System;

namespace Sawmill.Common.DateAndTime
{
    public class TimePeriod
    {
        public TimePeriod(DateTime start, TimeSpan duration)
        {
            this.Start = start;
            this.Duration = duration;
        }

        public TimePeriod(DateTime start, DateTime end)
        {
            this.Start = start;
            this.Duration = end - start;
        }

        public DateTime Start { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime End => this.Start + this.Duration;
    }
}
