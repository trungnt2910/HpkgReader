using HpkgReader.Heap;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HpkgReader.Extensions
{
    internal class HpkgHeapBuilder : IDisposable
    {
        /// <summary>
        /// The maximum chunk size.
        /// Because the size array at the end of the heap
        /// (the size of each chunk minus 1) can only hold
        /// values up to <see cref="ushort.MaxValue"/>,
        /// this should define the max chunk size.
        /// </summary>
        private const int MAX_CHUNK_SIZE = ushort.MaxValue + 1;

        private bool _disposedValue;
        private readonly string _tempDir;
        private readonly HeapCompression _compression;
        private readonly int _chunkSize;
        private readonly MemoryStream _currentChunk;
        private int _currentChunkIndex = 0;
        private int _totalSize = 0;
        private bool _completed = false;
        private ushort[] _chunkCompressedSizes;

        public int UncompressedSize => _totalSize;
        public int CompressedSize => 
            _compression == HeapCompression.NONE ? 
                _totalSize :
                // In the file:
                // The array is the compressed size of the chunk, minus 1.
                _chunkCompressedSizes.Select(i => (int)i + 1).Sum() +
                // The last chunk's info is omitted.
                sizeof(ushort) * (_chunkCompressedSizes.Length - 1);

        public HpkgHeapBuilder(string tempDir = null,
            HeapCompression compression = HeapCompression.ZLIB,
            int chunkSize = MAX_CHUNK_SIZE)
        {
            if (chunkSize > MAX_CHUNK_SIZE)
            {
                throw new InvalidOperationException($"Trying to create a heap with a chunk size that exceeded {MAX_CHUNK_SIZE}.");
            }

            _tempDir = tempDir ?? Path.Combine(Path.GetTempPath(), $"HpkgWriterTemp-{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDir);
            _compression = compression;
            _chunkSize = chunkSize;
            var buffer = new byte[_chunkSize];
            _currentChunk = new MemoryStream(buffer, 0, buffer.Length, true, true);
        }

        public int Write(byte[] data)
        {
            if (_completed)
            {
                throw new InvalidOperationException("Trying to write into a completed HeapBuilder.");
            }

            var dataOffset = 0;
            while (dataOffset < data.Length)
            {
                if (_currentChunk.Position == _chunkSize)
                {
                    var filePath = Path.Combine(_tempDir, _currentChunkIndex.ToString());
                    File.WriteAllBytes(filePath, _currentChunk.GetBuffer());
                    ++_currentChunkIndex;
                    _currentChunk.Position = 0;
                }

                var lengthToWrite =
                    Math.Min(data.Length - dataOffset, _chunkSize - (int)_currentChunk.Position);
                _currentChunk.Write(data, dataOffset, lengthToWrite);
                dataOffset += lengthToWrite;
            }
            _totalSize += data.Length;
            return data.Length;
        }

        public int Write(byte data)
        {
            return Write(new byte[] { data });
        }

        public int Write(sbyte data)
        {
            return Write(new byte[] { (byte)data });
        }

        // All small integers are stored in big endian format
        #region Small integers
        public int Write(short value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return Write(bytes);
        }

        public int Write(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return Write(bytes);
        }

        public int Write(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return Write(bytes);
        }

        public int Write(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return Write(bytes);
        }

        public int Write(long value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return Write(bytes);
        }

        public int Write(ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return Write(bytes);
        }
        #endregion

        /// <summary>
        /// Writes a <see cref="BigInteger"/> in the LEB128 format.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="signed">Whether to treat this as a signed integer.</param>
        /// <returns>The number of bytes written.</returns>
        public int Write(BigInteger value, bool signed = false)
        {
            if (signed)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (value.Sign < 0)
                {
                    throw new InvalidOperationException("Trying to write a negative integer in unsigned mode.");
                }
                // Write is quite expensive, we might want to cache stuff.
                var bytes = new List<byte>();
                do
                {
                    var currentByte = (byte)(value & 0x7f);
                    value >>= 7;
                    if (value != 0)
                    {
                        currentByte |= 0x80;
                    }
                    bytes.Add(currentByte);
                }
                while (value != 0);
                return Write(bytes.ToArray());
            }
        }

        /// <summary>
        /// Writes a <see cref="string"/> in UTF-8 encoding. 
        /// </summary>
        /// <param name="value">The string.</param>
        /// <returns>The number of bytes written.</returns>
        public int Write(string value)
        {
            int length = Write(Encoding.UTF8.GetBytes(value));
            // Null character.
            length += Write((byte)0);
            return length;
        }

        public int WriteAttributeTag(HpkgAttributeEncoding encoding, bool hasChildren, HpkgAttributeType dataType, int id)
        {
            var value =
                (new BigInteger((int)encoding) << 11)
                + ((new BigInteger(hasChildren ? 1 : 0)) << 10)
                + (new BigInteger((int)dataType) << 7)
                + id + 1;

            return Write(value);
        }

        public int WriteAttribute(BetterAttribute attribute)
        {
            int totalSize = 0;
            totalSize += WriteAttributeTag(attribute.Encoding, attribute.HasChildren, attribute.Type, attribute.Id.Code);
            switch (attribute.Type)
            {
                case HpkgAttributeType.INT:
                    switch (attribute.Encoding)
                    {
                        case HpkgAttributeEncoding.INT_8_BIT:
                            totalSize += Write((sbyte)attribute.Value);
                        break;
                        case HpkgAttributeEncoding.INT_16_BIT:
                            totalSize += Write((short)attribute.Value);
                        break;
                        case HpkgAttributeEncoding.INT_32_BIT:
                            totalSize += Write((int)attribute.Value);
                        break;
                        case HpkgAttributeEncoding.INT_64_BIT:
                            totalSize += Write((long)attribute.Value);
                        break;
                    }
                break;
                case HpkgAttributeType.UINT:
                    switch (attribute.Encoding)
                    {
                        case HpkgAttributeEncoding.INT_8_BIT:
                            totalSize += Write((byte)attribute.Value);
                        break;
                        case HpkgAttributeEncoding.INT_16_BIT:
                            totalSize += Write((ushort)attribute.Value);
                        break;
                        case HpkgAttributeEncoding.INT_32_BIT:
                            totalSize += Write((uint)attribute.Value);
                        break;
                        case HpkgAttributeEncoding.INT_64_BIT:
                            totalSize += Write((ulong)attribute.Value);
                        break;
                    }
                break;
                case HpkgAttributeType.STRING:
                    switch (attribute.Encoding)
                    {
                        case HpkgAttributeEncoding.STRING_INLINE:
                            totalSize += Write((string)attribute.Value);
                        break;
                        case HpkgAttributeEncoding.STRING_TABLE:
                        default:
                            throw new NotImplementedException();
                    }
                break;
                case HpkgAttributeType.RAW:
                    switch (attribute.Encoding)
                    {
                        case HpkgAttributeEncoding.RAW_INLINE:
                            totalSize += Write(new BigInteger(((byte[])attribute.Value).Length), false);
                            totalSize += Write((byte[])attribute.Value);
                        break;
                        case HpkgAttributeEncoding.RAW_HEAP:
                        default:
                            throw new NotImplementedException();
                    }
                break;
            }
            if (attribute.HasChildren)
            {
                totalSize += WriteAttributes(attribute.Children);
            }
            return totalSize;
        }

        public int WriteAttributes(IEnumerable<BetterAttribute> attributes)
        {
            int totalLength = 0;
            foreach (var attribute in attributes)
            {
                totalLength += WriteAttribute(attribute);
            }
            totalLength += Write(BigInteger.Zero, false);
            return totalLength;
        }

        /// <summary>
        /// Completes the creation of the heap.
        /// </summary>
        public void Complete()
        {
            _completed = true;
            if (_currentChunk.Position > 0)
            {
                var filePath = Path.Combine(_tempDir, _currentChunkIndex.ToString());
                _currentChunk.SetLength(_currentChunk.Position);
                File.WriteAllBytes(filePath, _currentChunk.ToArray());
                ++_currentChunkIndex;
            }
            switch (_compression)
            {
                case HeapCompression.NONE:
                return;
                case HeapCompression.ZLIB:
                {
                    _chunkCompressedSizes = new ushort[_currentChunkIndex];
                    var buffer = new byte[_chunkSize];
                    for (int i = 0; i < _currentChunkIndex; ++i)
                    {
                        var deflater = new Deflater(Deflater.BEST_COMPRESSION, false);
                        var fileName = Path.Combine(_tempDir, i.ToString());
                        deflater.SetInput(File.ReadAllBytes(fileName));
                        deflater.Finish();
                        int compressedSize = deflater.Deflate(buffer);
                        // Still more bytes? This means that compression actually
                        // made the chunk larger, so we should not use compression at all.
                        if (!deflater.IsFinished)
                        {
                            _chunkCompressedSizes[i] = (ushort)(_chunkSize - 1);
                        }
                        else
                        {
                            _chunkCompressedSizes[i] = (ushort)(compressedSize - 1);
                            var truncatedBuffer = new byte[compressedSize];
                            Array.Copy(buffer, truncatedBuffer, compressedSize);
                            File.WriteAllBytes(fileName, truncatedBuffer);
                        }
                    }
                }
                return;
                default:
                throw new InvalidOperationException($"Invalid compression type: {_compression}");
            }
        }

        public void WriteToStream(Stream s)
        {
            if (!_completed)
            {
                throw new InvalidOperationException("Cannot dump incomplete heap.");
            }
            for (int i = 0; i < _currentChunkIndex; ++i)
            {
                var bytes = File.ReadAllBytes(Path.Combine(_tempDir, i.ToString()));
                s.Write(bytes, 0, bytes.Length);
            }
            if (_compression != HeapCompression.NONE)
            {
                for (int i = 0; i + 1 < _currentChunkIndex; ++i)
                {
                    s.WriteBigEndian(_chunkCompressedSizes[i]);
                }
            }
        }

        /// <summary>
        /// Gets the chunk compressed size info.
        /// This is usually the array at the end of the heap of a hpkg.
        /// </summary>
        /// <returns>An array containing the compressed chunk size info.</returns>
        public ushort[] GetChunkCompressedSizeInfo()
        {
            if (!_completed)
            {
                throw new InvalidOperationException("The heap has not been completed.");
            }

            if (_compression == HeapCompression.NONE)
            {
                return Array.Empty<ushort>();
            }

            return _chunkCompressedSizes;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Directory.Delete(_tempDir, true);
                    _currentChunk.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HeapBuilder()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
