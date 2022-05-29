/*
 * Copyright 2018-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

//import com.google.common.cache.CacheBuilder;
//import com.google.common.cache.CacheLoader;
//import com.google.common.cache.LoadingCache;
//import org.haiku.pkg.FileHelper;

//import java.io.Closeable;
//import java.io.File;
//import java.io.IOException;
//import java.io.RandomAccessFile;
//import java.util.zip.DataFormatException;
//import java.util.zip.Inflater;

using HpkgReader.Compat;
using System;
using System.IO;

namespace HpkgReader.Heap
{
    /**
     * <P>An instance of this class is able to read the heap's chunks that are in HPK format.  Note
     * that this class will also take responsibility for caching the chunks so that a subsequent
     * read from the same chunk will not require a re-fault from disk.</P>
     */

    public class HpkHeapReader : HeapReader, IDisposable
    {

        private readonly HeapCompression compression;

        private readonly long heapOffset;

        private readonly long chunkSize;

        private readonly long compressedSize; // including the shorts for the chunks' compressed sizes

        private readonly long uncompressedSize; // excluding the shorts for the chunks' compressed sizes

        private readonly LoadingCache<int, byte[]> heapChunkUncompressedCache;

        private readonly int[] heapChunkCompressedLengths;

        private readonly FileStream randomAccessFile;

        private readonly FileHelper fileHelper = new FileHelper();

        public HpkHeapReader(
                FileInfo file,
                HeapCompression compression,
                long heapOffset,
                long chunkSize,
                long compressedSize,
                long uncompressedSize)
        {
            Preconditions.CheckNotNull(file);
            //Preconditions.CheckNotNull(compression);
            Preconditions.CheckState(heapOffset > 0 && heapOffset < int.MaxValue);
            Preconditions.CheckState(chunkSize > 0 && chunkSize < int.MaxValue);
            Preconditions.CheckState(compressedSize >= 0 && compressedSize < int.MaxValue);
            Preconditions.CheckState(uncompressedSize >= 0 && uncompressedSize < int.MaxValue);

            this.compression = compression;
            this.heapOffset = heapOffset;
            this.chunkSize = chunkSize;
            this.compressedSize = compressedSize;
            this.uncompressedSize = uncompressedSize;

            try
            {
                randomAccessFile = RandomAccessFileCompat.Construct(file, "r");

                heapChunkCompressedLengths = new int[GetHeapChunkCount()];
                PopulateChunkCompressedLengths(heapChunkCompressedLengths);

                heapChunkUncompressedCache = CacheBuilder<int, byte[]>
                        .NewBuilder()
                        .MaximumSize(3)
                        .Build(new CacheLoader<int, byte[]>()
                        {
                            Load = (int key) =>
                            {
                                //Preconditions.CheckNotNull(key);

                                // TODO: best to avoid continuously allocating new byte buffers
                                byte[] result = new byte[GetHeapChunkUncompressedLength(key)];
                                ReadHeapChunk(key, result);
                                return result;
                            }
                        });
            }
            catch (Exception e)
            {
                Close();
                throw new HpkException("unable to configure the hpk heap reader", e);
            }
        }

        public void Close()
        {
            if (null != randomAccessFile)
            {
                try
                {
                    randomAccessFile.Close();
                }
                catch (IOException)
                {
                    // ignore
                }
            }
        }

        public void Dispose()
        {
            randomAccessFile?.Dispose();
        }


        /// <summary>
        /// This gives the quantity of chunks that are in the heap.
        /// </summary>
        private int GetHeapChunkCount()
        {
            int count = (int)(uncompressedSize / chunkSize);

            if (0 != uncompressedSize % chunkSize)
            {
                count++;
            }

            return count;
        }

        private int GetHeapChunkUncompressedLength(int index)
        {
            if (index < GetHeapChunkCount() - 1)
            {
                return (int)chunkSize;
            }

            return (int)(uncompressedSize - (chunkSize * (GetHeapChunkCount() - 1)));
        }

        private int GetHeapChunkCompressedLength(int index)
        {
            return heapChunkCompressedLengths[index];
        }

        /// <summary>
        /// After the chunk data is a whole lot of unsigned shorts that define the compressed
        /// size of the chunks in the heap.This method will shift the input stream to the
        /// start of those shorts and read them in.
        /// </summary>
        private void PopulateChunkCompressedLengths(int[] lengths)
        {
            Preconditions.CheckNotNull(lengths);

            int count = GetHeapChunkCount();
            long totalCompressedLength = 0;
            randomAccessFile.Seek(heapOffset + compressedSize - (2 * ((long)count - 1)));

            for (int i = 0; i < count - 1; i++)
            {

                // C++ code says that the stored size is length of chunk -1.
                lengths[i] = fileHelper.ReadUnsignedShortToInt(randomAccessFile) + 1;

                if (lengths[i] > uncompressedSize)
                {
                    throw new HpkException(
                            string.Format("the chunk at {0} is of size {1}, but the uncompressed length of the chunks is {2}",
                                    i,
                                    lengths[i],
                                    uncompressedSize));
                }

                totalCompressedLength += lengths[i];
            }

            // the last one will be missing will need to be derived
            lengths[count - 1] = (int)(compressedSize - ((2 * (count - 1)) + totalCompressedLength));

            if (lengths[count - 1] < 0 || lengths[count - 1] > uncompressedSize)
            {
                throw new HpkException(
                        string.Format(
                                "the derivation of the last chunk size of {0} is out of bounds",
                                lengths[count - 1]));
            }
        }

        private bool IsHeapChunkCompressed(int index)
        {
            return GetHeapChunkCompressedLength(index) < GetHeapChunkUncompressedLength(index);
        }

        private long GetHeapChunkAbsoluteFileOffset(int index)
        {
            long result = heapOffset; // heap comes after the header.

            for (int i = 0; i < index; i++)
            {
                result += GetHeapChunkCompressedLength(i);
            }

            return result;
        }


        /// <summary>
        /// This will read from the current offset into the supplied buffer until the supplied buffer is completely
        /// filledup.
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="HpkException"></exception>
        private void ReadFully(byte[] buffer)
        {
            Preconditions.CheckNotNull(buffer);
            int total = 0;

            while (total < buffer.Length)
            {
                int read = randomAccessFile.Read(buffer, total, buffer.Length - total);

                if (-1 == read)
                {
                    throw new HpkException("unexpected end of file when reading a chunk");
                }

                total += read;
            }
        }

        /// <summary>
        /// This will read a chunk of the heap into the supplied buffer. It is assumed that the buffer will be
        /// of the correct length for the uncompressed heap chunk size.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="buffer"></param>
        /// <exception cref="IllegalStateException"></exception>
        /// <exception cref="HpkException"></exception>
        private void ReadHeapChunk(int index, byte[] buffer)
        {

            randomAccessFile.Seek(GetHeapChunkAbsoluteFileOffset(index));
            int chunkUncompressedLength = GetHeapChunkUncompressedLength(index);

            if (IsHeapChunkCompressed(index) || HeapCompression.NONE == compression)
            {

                switch (compression)
                {
                    case HeapCompression.NONE:
                        throw new InvalidOperationException();

                    case HeapCompression.ZLIB:
                        {
                            byte[] deflatedBuffer = new byte[GetHeapChunkCompressedLength(index)];
                            ReadFully(deflatedBuffer);

                            Inflater inflater = new Inflater();
                            inflater.SetInput(deflatedBuffer);

                            try
                            {
                                int read;

                                if (chunkUncompressedLength != (read = inflater.Inflate(buffer)))
                                {

                                    // the last chunk size uncompressed may be smaller than the chunk size,
                                    // so don't throw an exception if this happens.

                                    if (index < GetHeapChunkCount() - 1)
                                    {
                                        string message = string.Format("a compressed heap chunk inflated to {0} bytes; was expecting {1}", read, chunkUncompressedLength);

                                        if (inflater.NeedsInput())
                                        {
                                            message += "; needs input";
                                        }

                                        if (inflater.NeedsDictionary())
                                        {
                                            message += "; needs dictionary";
                                        }

                                        throw new HpkException(message);
                                    }
                                }

                                if (!inflater.Finished())
                                {
                                    throw new HpkException(string.Format("incomplete inflation of input data while reading chunk {0}", index));
                                }
                            }
                            catch (InvalidDataException dfe)
                            {
                                throw new HpkException("unable to inflate (decompress) heap chunk " + index, dfe);
                            }
                        }
                        break;

                    default:
                        throw new InvalidOperationException("unsupported compression; " + compression);
                }
            }
            else
            {
                int read;

                if (chunkUncompressedLength != (read = randomAccessFile.Read(buffer, 0, chunkUncompressedLength)))
                {
                    throw new HpkException(string.Format("problem reading chunk {0} of heap; only read {1} of {2} bytes", index, read, buffer.Length));
                }
            }
        }

        public override int ReadHeap(long offset)
        {
            Preconditions.CheckState(offset >= 0);
            Preconditions.CheckState(offset < uncompressedSize);

            int chunkIndex = (int)(offset / chunkSize);
            int chunkOffset = (int)(offset - (chunkIndex * chunkSize));
            byte[] chunkData = heapChunkUncompressedCache.GetUnchecked(chunkIndex);

            return chunkData[chunkOffset] & 0xff;
        }

        public override void ReadHeap(byte[] buffer, int bufferOffset, HeapCoordinates coordinates)
        {

            Preconditions.CheckNotNull(buffer);
            Preconditions.CheckState(bufferOffset >= 0);
            Preconditions.CheckState(bufferOffset < buffer.Length);
            Preconditions.CheckState(coordinates.Offset >= 0);
            Preconditions.CheckState(coordinates.Offset < uncompressedSize);
            Preconditions.CheckState(coordinates.Offset + coordinates.Length < uncompressedSize);

            // first figure out how much to read from this chunk

            int chunkIndex = (int)(coordinates.Offset / chunkSize);
            int chunkOffset = (int)(coordinates.Offset - (chunkIndex * chunkSize));
            int chunkLength;
            int chunkUncompressedLength = GetHeapChunkUncompressedLength(chunkIndex);

            if (chunkOffset + coordinates.Length > chunkUncompressedLength)
            {
                chunkLength = (chunkUncompressedLength - chunkOffset);
            }
            else
            {
                chunkLength = (int)coordinates.Length;
            }

            // now read it in.

            byte[] chunkData = heapChunkUncompressedCache.GetUnchecked(chunkIndex);

            Array.Copy(chunkData, chunkOffset, buffer, bufferOffset, chunkLength);

            // if we need to get some more data from the next chunk then call again.
            // TODO - recursive approach may not be too good when more data is involved; probably ok for hpkr though.

            if (chunkLength < coordinates.Length)
            {
                ReadHeap(
                        buffer,
                        bufferOffset + chunkLength,
                        new HeapCoordinates(
                                coordinates.Offset + chunkLength,
                                coordinates.Length - chunkLength));
            }

        }

    }

}