// Credit: https://github.com/erdomke/RangeTree
using System;
using System.Collections.Generic;

namespace ArcCreate.Utility.RangeTree
{
    /// <summary>
    /// Represents a range of values.
    /// Both values must be of the same type and comparable.
    /// </summary>
    /// <typeparam name="TKey">Type of the values.</typeparam>
    public struct RangeValuePair<TKey, TValue> : IEquatable<RangeValuePair<TKey, TValue>>
    {
        public TKey From { get; }
        public TKey To { get; }
        public TValue Value { get; }

        /// <summary>
        /// Initializes a new <see cref="RangeValuePair&lt;TKey, TValue&gt;"/> instance.
        /// </summary>
        public RangeValuePair(TKey from, TKey to, TValue value) : this()
        {
            From = from;
            To = to;
            Value = value;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("[{0} - {1}] {2}", From, To, Value);
        }

        public override int GetHashCode()
        {
            var hash = 23;
            if (From != null)
                hash = hash * 37 + From.GetHashCode();
            if (To != null)
                hash = hash * 37 + To.GetHashCode();
            if (Value != null)
                hash = hash * 37 + Value.GetHashCode();
            return hash;
        }

        public bool Equals(RangeValuePair<TKey, TValue> other)
        {
            return EqualityComparer<TKey>.Default.Equals(From, other.From)
                && EqualityComparer<TKey>.Default.Equals(To, other.To)
                && EqualityComparer<TValue>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RangeValuePair<TKey, TValue>))
                return false;

            return Equals((RangeValuePair<TKey, TValue>)obj);
        }

        public static bool operator ==(RangeValuePair<TKey, TValue> left, RangeValuePair<TKey, TValue> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RangeValuePair<TKey, TValue> left, RangeValuePair<TKey, TValue> right)
        {
            return !(left == right);
        }
    }
}