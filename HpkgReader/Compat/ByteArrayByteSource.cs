using System;
using System.IO;

namespace HpkgReader.Compat
{
    internal class ByteArrayByteSource : ByteSource
    {
        private readonly byte[] _value;

        public ByteArrayByteSource(byte[] value)
        {
            _value = value;
        }

        public override Stream OpenStream()
        {
            return new MemoryStream(_value, false);
        }

        public override long? SizeIfKnown()
        {
            return _value.LongLength;
        }

        public override long Size()
        {
            return _value.LongLength;
        }

        public override byte[] Read()
        {
            var arr = new byte[_value.Length];
            Array.Copy(_value, arr, _value.Length);
            return arr;
        }
    }
}
