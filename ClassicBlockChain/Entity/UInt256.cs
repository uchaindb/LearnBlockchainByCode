using System;
using System.Diagnostics;
using System.Linq;

namespace UChainDB.Example.Chain.Entity
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public class UInt256 : IComparable<UInt256>
    {
        public static readonly UInt256 Zero = new UInt256(Enumerable.Repeat((byte)0, 32).ToArray());
        private readonly byte[] data;

        public UInt256(byte[] d)
        {
            this.data = d;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected string DebuggerDisplay => this.ToShort();

        public static bool operator ==(UInt256 left, UInt256 right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(UInt256 left, UInt256 right)
        {
            return !(left == right);
        }

        public static implicit operator byte[] (UInt256 value)
        {
            return value == null ? null : value.data;
        }

        public static UInt256 Parse(string str)
        {
            return new UInt256(Convert.FromBase64String(str));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UInt256))
                return false;

            var other = (UInt256)obj;
            return this.data.SequenceEqual(other.data);
        }

        public override int GetHashCode()
        {
            var hashCodeStart = BitConverter.ToInt32(this.data, 0);
            var hashCodeMedium = BitConverter.ToInt32(this.data, 8);
            var hashCodeEnd = BitConverter.ToInt32(this.data, 24);
            return hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
        }

        public override string ToString()
        {
            return this.ToShort();
        }

        public string ToHex()
        {
            return BitConverter.ToString(this.data).Replace("-", string.Empty);
        }

        public int CompareTo(UInt256 other)
        {
            return this.data.SequenceEqual(other.data) ? 0 : 1;
        }
    }
}