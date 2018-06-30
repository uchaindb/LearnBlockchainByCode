﻿using System;

namespace UChainDB.Example.BlockChain.Chain
{
    public static class Extension
    {
        public static string ToShort(this UInt256 hash, int len = 7)
        {
            return hash.ToBase64().Substring(0, len);
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