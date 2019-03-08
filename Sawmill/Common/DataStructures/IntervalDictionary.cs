using Sawmill.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sawmill.Common.DataStructures
{
    public class IntervalDictionary<T> : IEnumerable<T>
    {
        public IntervalDictionary(DateTime start, TimeSpan duration, TimeSpan interval)
            : this(start, interval, (int)(duration.Ticks / interval.Ticks))
        {
        }

        public IntervalDictionary(DateTime start, TimeSpan interval, int count)
        {
            this.Interval = interval.Ticks >= 0
                ? interval
                : throw new ArgumentOutOfRangeException(nameof(interval));

            this.StartUtc = start.ToUniversalTime().Floor(interval);

            this.Data = new T[count];
            this.IndexOffset = 0;
        }

        private T[] Data { get; set; }
        private int IndexOffset { get; set; }

        public int Count => this.Data.Length;

        public TimeSpan Interval { get; }

        public DateTime StartUtc { get; private set; }
        public DateTime EndUtc => this.StartUtc + this.Interval * this.Count;

        public T this[DateTime dateTime]
        {
            get => this.GetData(dateTime);
            set => this.SetData(dateTime, value);
        }

        public void MoveStartDate(DateTime newStart, Func<T, T> reset = null)
        {
            var newStartUtc = newStart.ToUniversalTime().Floor(this.Interval);
            var timeOffset = newStartUtc - this.StartUtc;
            var offset = (int)(timeOffset.Ticks / this.Interval.Ticks);
            this.ShiftArray(offset, reset);
            this.StartUtc = newStartUtc;
        }

        private void ShiftArray(int offset, Func<T, T> reset = null)
        {
            if (offset <= -this.Count || offset >= this.Count)
            {
                this.Reset(reset);
                return;
            }
            else if (offset < 0)
            {
                var arrayIndex = this.IndexOffset + offset;
                if (arrayIndex < 0)
                {
                    arrayIndex += this.Count;
                }

                this.Reset(arrayIndex, -offset, reset);
            }
            else if (offset == 0)
            {
                return;
            }
            else if(offset < this.Count)
            {
                this.Reset(this.IndexOffset, offset, reset);
            }

            this.ShiftIndex(offset);
        }

        private void ShiftIndex(int offset)
        {
            var newOffset = this.IndexOffset + offset;

            if(newOffset >= this.Count)
            {
                newOffset = newOffset % this.Count;
            }
            else if (newOffset < 0)
            {
                newOffset = this.Count - 1 - ((-1 - newOffset) % this.Count);
            }

            this.IndexOffset = newOffset;
        }

        private void Reset(Func<T, T> reset = null)
        {
            this.Reset(this.IndexOffset, this.Count, reset);
            this.IndexOffset = 0;
        }

        private void Reset(int arrayIndex, int count, Func<T, T> reset = null)
        {
            if(count < 0 || count > this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            int tailCount;
            int headCount;

            if(arrayIndex + count > this.Count)
            {
                tailCount = this.Count - arrayIndex;
                headCount = count - tailCount;
            }
            else
            {
                tailCount = count;
                headCount = 0;
            }

            if (reset != null)
            {
                for (var i = arrayIndex; i < arrayIndex + tailCount; i++)
                {
                    this.Data[i] = reset(this.Data[i]);
                }

                for (var i = 0; i < headCount; i++)
                {
                    this.Data[i] = reset(this.Data[i]);
                }
            }
            else
            {
                Array.Clear(this.Data, arrayIndex, tailCount);
                Array.Clear(this.Data, 0, headCount);
            }
        }

        private T GetData(DateTime date)
        {
            return this.GetDataAtIndex(this.GetIndex(date));
        }

        private void SetData(DateTime date, T value)
        {
            this.SetDataAtIndex(this.GetIndex(date), value);
        }

        private T GetDataAtIndex(int index)
        {
            var arrayIndex = this.GetArrayIndex(index);
            return this.Data[arrayIndex];
        }

        private void SetDataAtIndex(int index, T value)
        {
            var arrayIndex = this.GetArrayIndex(index);
            this.Data[arrayIndex] = value;
        }

        private (DateTime startUtc, DateTime endUtc) GetIndexPeriod(int index)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var periodStartUtc = this.StartUtc + index * this.Interval;
            var periodEndUtc = periodStartUtc + this.Interval;

            return (periodStartUtc, periodEndUtc);
        }

        private int GetIndex(DateTime date)
        {
            var dateUtc = date.ToUniversalTime();
            if (dateUtc < this.StartUtc || dateUtc >= this.EndUtc)
            {
                return -1;
            }

            var dateFloor = dateUtc.Floor(this.Interval);
            return (int)((dateFloor - this.StartUtc).Ticks / this.Interval.Ticks);
        }

        private int GetArrayIndex(DateTime date)
        {
            return this.GetArrayIndex(this.GetIndex(date));
        }

        private int GetArrayIndex(int index)
        {
            if(index < 0 || index >= this.Count)
            {
                return -1;
            }

            return (index + this.IndexOffset) % this.Count;
        }

        private int GetIndex(int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex >= this.Count)
            {
                return -1;
            }

            var index = arrayIndex - this.IndexOffset;
            return index >= 0 ? index : index + this.Count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.Data.Skip(this.IndexOffset).Concat(this.Data.Take(this.IndexOffset)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
