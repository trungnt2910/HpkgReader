/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using HpkgReader.Model;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace HpkgReader
{
    /// <summary>
    /// This helps out with typical common reads that might be performed as part of
    /// parsing various values in the HPKR file.
    /// </summary>
    public class FileHelper
    {
        private readonly static BigInteger MAX_BIGINTEGER_FILE = new BigInteger(long.MaxValue);

        private readonly byte[] buffer8 = new byte[8];

        public FileType GetType(FileStream file)
        {
            return TryGetType(file) ?? throw new HpkException("unable to establish the file type");
        }

        public FileType? TryGetType(FileStream file)
        {
            string magicString = new string(ReadMagic(file));
            return Enum.GetValues(typeof(FileType))
                .Cast<FileType>()
                .FirstOrDefault(e => e.ToString().ToLowerInvariant() == magicString);
        }

        public int ReadUnsignedShortToInt(FileStream randomAccessFile)
        {

            if (2 != randomAccessFile.Read(buffer8, 0, 2))
            {
                throw new HpkException("not enough bytes read for an unsigned short");
            }

            int i0 = buffer8[0] & 0xff;
            int i1 = buffer8[1] & 0xff;

            return i0 << 8 | i1;
        }

        public long ReadUnsignedIntToLong(FileStream randomAccessFile)
        {

            if (4 != randomAccessFile.Read(buffer8, 0, 4))
            {
                throw new HpkException("not enough bytes read for an unsigned int");
            }

            long l0 = buffer8[0] & 0xff;
            long l1 = buffer8[1] & 0xff;
            long l2 = buffer8[2] & 0xff;
            long l3 = buffer8[3] & 0xff;

            return l0 << 24 | l1 << 16 | l2 << 8 | l3;
        }

        public BigInteger ReadUnsignedLong(FileStream randomAccessFile)
        {

            if (8 != randomAccessFile.Read(buffer8, 0, 8))
            {
                throw new HpkException("not enough bytes read for an unsigned long");
            }

            return BigIntegerCompat.Construct(1, buffer8);
        }

        public long ReadUnsignedLongToLong(FileStream randomAccessFile)
        {

            BigInteger result = ReadUnsignedLong(randomAccessFile);

            if (result.CompareTo(MAX_BIGINTEGER_FILE) > 0)
            {
                throw new HpkException("the hpkr file contains an unsigned long which is larger than can be represented in a java long");
            }

            return (long)result;
        }

        public char[] ReadMagic(FileStream randomAccessFile)
        {

            if (4 != randomAccessFile.Read(buffer8, 0, 4))
            {
                throw new HpkException("not enough bytes read for a 4-byte magic");
            }

            return new char[] 
            {
                (char) buffer8[0],
                (char) buffer8[1],
                (char) buffer8[2],
                (char) buffer8[3]
            };
        }
    }
}