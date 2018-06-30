using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace UChainDB.Example.Chain.Entity
{
    public class Hash
    {
        private readonly byte[] data;

        public Hash(byte[] origin)
        {
            this.data = SHA256Hash(origin);
        }

        public Hash(IEnumerable<byte[]> origins)
        {
            this.data = SHA256Hash(origins);
        }

        public Hash(string origin)
            : this(Encoding.UTF8.GetBytes(origin))
        {
        }

        public static implicit operator UInt256(Hash value)
        {
            return new UInt256(value.data);
        }

        public static implicit operator byte[] (Hash value)
        {
            return value.data;
        }

        private static byte[] SHA256Hash(byte[] bytes)
        {
            using (var hash = SHA256.Create())
            {
                return hash.ComputeHash(bytes);
            }
        }

        private static byte[] SHA256Hash(IEnumerable<byte[]> bytesArray)
        {
            if (bytesArray == null)
            {
                throw new ArgumentNullException(nameof(bytesArray));
            }

            using (var hash = SHA256.Create())
            {
                var e = bytesArray.GetEnumerator();
                while (e.MoveNext())
                {
                    var arr = e.Current;
                    hash.TransformBlock(arr, 0, arr.Length, null, 0);
                }
                hash.TransformFinalBlock(new byte[] { }, 0, 0);
                return hash.Hash;
            }
        }
    }
}