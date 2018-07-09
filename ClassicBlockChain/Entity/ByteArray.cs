using System;
using System.Diagnostics;
using System.Linq;
using UChainDB.Example.Chain.Utility;

namespace UChainDB.Example.Chain.Entity
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public abstract class ByteArray<T> where T : ByteArrayDef, new()
    {
        protected readonly byte[] bytes;
        protected readonly T Def = new T();

        public ByteArray(byte[] byteArray)
        {
            if (byteArray == null)
            {
                throw new ArgumentNullException("Should not be null", nameof(byteArray));
            }

            if (byteArray.Length != this.Def.Length)
            {
                throw new ArgumentException($"Length should be {this.Def.Length}", nameof(byteArray));
            }

            this.bytes = byteArray;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected string DebuggerDisplay => this.ToBase58().Substring(0, 7);

        public static implicit operator byte[] (ByteArray<T> obj)
        {
            return obj?.bytes;
        }

        public string ToBase58()
        {
            return Base58.Encode(this.bytes);
        }

        public static bool operator ==(ByteArray<T> left, ByteArray<T> right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(ByteArray<T> left, ByteArray<T> right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ByteArray<T>))
                return false;

            var other = (ByteArray<T>)obj;
            return this.bytes.SequenceEqual(other.bytes);
        }

        public override int GetHashCode()
        {
            var data = this.bytes;
            // borrow from https://stackoverflow.com/a/468084
            unchecked
            {
                const int p = 16777619;
                int hash = (int)2166136261;

                for (int i = 0; i < data.Length; i++)
                    hash = (hash ^ data[i]) * p;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }
    }

    public abstract class ByteArrayDef
    {
        public abstract int Length { get; }
    }
}