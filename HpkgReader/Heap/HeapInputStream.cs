/*
 * Copyright 2021, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using System;
using System.IO;

namespace HpkgReader.Heap
{
    public class HeapInputStream : Stream
    {
        private readonly HeapReader reader;

        private readonly HeapCoordinates coordinates;

        private long offsetInCoordinates = 0L;

        public HeapInputStream(HeapReader reader, HeapCoordinates coordinates)
        {
            this.reader = Preconditions.CheckNotNull(reader);
            this.coordinates = Preconditions.CheckNotNull(coordinates);
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] b, int off, int len)
        {
            Preconditions.CheckArgument(null != b, "buffer required");
            Preconditions.CheckArgument(off >= 0, "bad offset supplied");
            Preconditions.CheckArgument(len >= 0, "bad length supplied");

            if (len + offsetInCoordinates >= coordinates.Length)
            {
                len = (int)(coordinates.Length - offsetInCoordinates);
            }

            if (0 == len)
            {
                // .NET streams should return 0 on read failure.
                return 0;
                //return -1;
            }

            HeapCoordinates readCoordinates = new HeapCoordinates(
                    coordinates.Offset + offsetInCoordinates, len);

            reader.ReadHeap(b, off, readCoordinates);
            offsetInCoordinates += len;

            return len;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    offsetInCoordinates = offset;
                    break;
                case SeekOrigin.Current:
                    offsetInCoordinates += offset;
                    break;
                case SeekOrigin.End:
                    offsetInCoordinates = coordinates.Length + offset;
                    break;
            }
            return offsetInCoordinates;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => coordinates.Length;

        public override long Position
        {
            get => offsetInCoordinates;
            set => offsetInCoordinates = value;
        }
        
        public int Read()
        {
            if (offsetInCoordinates < coordinates.Length)
            {
                int result = reader.ReadHeap(coordinates.Offset + offsetInCoordinates);
                offsetInCoordinates++;
                return result;
            }

            return -1;
        }
    }
}
