using System;
using System.IO;

namespace HpkgReader.Extensions
{
    internal static class StreamExtensions
    {
        public static void WriteBigEndian(this Stream ms, ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            ms.Write(bytes, 0, bytes.Length);
        }

        public static void WriteBigEndian(this Stream ms, uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            ms.Write(bytes, 0, bytes.Length);
        }

        public static void WriteBigEndian(this Stream ms, ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            ms.Write(bytes, 0, bytes.Length);
        }
    }
}
