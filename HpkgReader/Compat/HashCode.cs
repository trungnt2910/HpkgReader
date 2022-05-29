﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HpkgReader.Compat
{
    public class HashCode
    {
        private readonly byte[] _data;
        internal HashCode(byte[] data)
        {
            _data = data;
        }

        public override string ToString()
        {
            return ByteArrayToHexViaLookup32(_data);
        }

        #region ToHexString
        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        private static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }
        #endregion
    }
}
