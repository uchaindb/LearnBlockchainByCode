namespace UChainDB.Example.Chain.Utility
{
    using System;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Modified from CodesInChaos' public domain code
    /// https://gist.github.com/CodesInChaos/3175971
    /// and MIT code: https://github.com/Chainers/Cryptography.ECDSA
    /// </summary>
    public static class Base58
    {
        public const int CheckSumSizeInBytes = 4;
        private const string Hexdigits = "0123456789abcdefABCDEF";
        private const string Digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        public static string EncodeWithCheckSum(byte[] data)
        {
            return Encode(AddCheckSum(data));
        }

        public static bool DecodeWithCheckSum(string base58, out byte[] decoded)
        {
            var dataWithCheckSum = Decode(base58);
            var success = VerifyCheckSum(dataWithCheckSum);
            decoded = RemoveCheckSum(dataWithCheckSum);
            return success;
        }

        public static string Encode(byte[] data)
        {
            // Decode byte[] to BigInteger
            BigInteger intData = 0;
            for (var i = 0; i < data.Length; i++)
            {
                intData = intData * 256 + data[i];
            }

            // Encode BigInteger to Base58 string
            var result = "";
            while (intData > 0)
            {
                var remainder = (int)(Mod(intData, 58));
                intData /= 58;
                result = Digits[remainder] + result;
            }

            // Append `1` for each leading 0 byte
            for (var i = 0; i < data.Length && data[i] == 0; i++)
            {
                result = '1' + result;
            }
            return result;
        }

        public static byte[] Decode(string base58)
        {
            // Decode Base58 string to BigInteger
            BigInteger intData = 0;
            for (var i = 0; i < base58.Length; i++)
            {
                var digit = Digits.IndexOf(base58[i]); //Slow
                if (digit < 0)
                    throw new FormatException($"Invalid Base58 character `{base58[i]}` at position {i}");
                intData = intData * 58 + digit;
            }

            // Encode BigInteger to byte[]
            // Leading zero bytes get encoded as leading `1` characters
            var leadingZeroCount = base58.TakeWhile(c => c == '1').Count();
            var leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
            var bytesWithoutLeadingZeros =
                intData.ToByteArray()
                .Reverse()// to big endian
                .SkipWhile(b => b == 0);//strip sign byte
            var result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();
            return result;
        }

        internal static string Encode(string hexString)
        {
            if (!hexString.All(Hexdigits.Contains))
            {
                throw new ArgumentOutOfRangeException(nameof(hexString), "Only hex string are allowed");
            }

            return Encode(HexToBytes(hexString));
        }

        private static byte[] HexToBytes(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private static byte[] RemoveCheckSum(byte[] data)
        {
            var result = new byte[data.Length - CheckSumSizeInBytes];
            Buffer.BlockCopy(data, 0, result, 0, data.Length - CheckSumSizeInBytes);

            return result;
        }

        private static bool VerifyCheckSum(byte[] data)
        {
            var result = new byte[data.Length - CheckSumSizeInBytes];
            Buffer.BlockCopy(data, 0, result, 0, data.Length - CheckSumSizeInBytes);
            var correctCheckSum = GetCheckSum(result);
            for (var i = CheckSumSizeInBytes; i >= 1; i--)
            {
                if (data[data.Length - i] != correctCheckSum[CheckSumSizeInBytes - i])
                {
                    return false;
                }
            }
            return true;
        }

        private static BigInteger Mod(BigInteger value, BigInteger divVal)
        {
            var rez = value % divVal;
            if (value.Sign < 0 && divVal.Sign > 0 && rez.Sign < 0)
                return divVal + rez;
            return rez;
        }

        private static byte[] AddCheckSum(byte[] data)
        {
            var checkSum = GetCheckSum(data);

            var result = new byte[checkSum.Length + data.Length];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            Buffer.BlockCopy(checkSum, 0, result, data.Length, checkSum.Length);
            return result;
        }

        private static byte[] GetCheckSum(byte[] data)
        {
            var hash = DoubleHash(data);
            Array.Resize(ref hash, CheckSumSizeInBytes);
            return hash;
        }

        private static byte[] DoubleHash(byte[] data)
        {
            return ComputeHash(ComputeHash(data));
        }

        private static byte[] ComputeHash(byte[] bytes)
        {
            using (var hash = System.Security.Cryptography.SHA256.Create())
            {
                return hash.ComputeHash(bytes);
            }
        }
    }
}