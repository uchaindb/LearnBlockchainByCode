using System;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.Utility
{
    public static class Extension
    {
        public static string ToShort(this UInt256 hash, int len = 10)
        {
            return hash.ToHex().Substring(0, len);
        }

        public static long ToUnixTimestamp(this DateTime time)
        {
            if (time == DateTime.MinValue) return -1;
            return ((DateTimeOffset)time).ToUnixTimeMilliseconds();
        }

        public static DateTime ToDateTime(this long unixTimestamp)
        {
            var dt = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp).LocalDateTime;
            return dt;
        }
    }
}