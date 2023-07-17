// Credit: https://github.com/erdomke/RangeTree
using System;
using System.Collections.Generic;

namespace ArcCreate.Utility.RangeTree
{
    /// <summary>
    /// Represents a range of values.
    /// Both values must be of the same type and comparable.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public readonly struct RangeValuePair<T> : IEquatable<RangeValuePair<T>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RangeValuePair{T}"/> struct.
        /// </summary>
        /// <param name="from">Lower end of the node's range.</param>
        /// <param name="to">Upper end of the node's range.</param>
        /// <param name="value">Value of the node.</param>
        public RangeValuePair(double from, double to, T value)
            : this()
        {
            From = from;
            To = to;
            Value = value;
        }

        public double From { get; }

        public double To { get; }

        public T Value { get; }

        public static bool operator ==(RangeValuePair<T> left, RangeValuePair<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RangeValuePair<T> left, RangeValuePair<T> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("[{0} - {1}] {2}", From, To, Value);
        }

        public override int GetHashCode()
        {
            var hash = 23;
            hash = (hash * 37) + From.GetHashCode();
            hash = (hash * 37) + To.GetHashCode();

            if (Value != null)
            {
                hash = (hash * 37) + Value.GetHashCode();
            }

            return hash;
        }

        public bool Equals(RangeValuePair<T> other)
        {
            return EqualityComparer<double>.Default.Equals(From, other.From)
                && EqualityComparer<double>.Default.Equals(To, other.To)
                && EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RangeValuePair<T>))
            {
                return false;
            }

            return Equals((RangeValuePair<T>)obj);
        }
    }
}