/*
 * Copyright 2013-2021, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;

namespace HpkgReader.Heap
{
    /// <summary>
    /// This object provides an offset and length into the heap and this provides a coordinate for a chunk of
    /// data in the heap. Note that the coordinates refer to the uncompressed data across all of the chunks of the heap.
    /// </summary>
    public class HeapCoordinates
    {
        private readonly long offset;
        private readonly long length;

        public HeapCoordinates(long offset, long length)
        {
            Preconditions.CheckState(offset >= 0 && offset < int.MaxValue);
            Preconditions.CheckState(length >= 0 && length < int.MaxValue);

            this.offset = offset;
            this.length = length;
        }

        public long Offset => offset;

        public long Length => length;

        public bool Empty => length == 0;

        public override bool Equals(object o)
        {
            if (this == o) return true;

            if (o == null || GetType() != o.GetType()) return false;

            HeapCoordinates that = (HeapCoordinates)o;

            if (length != that.length) return false;
            if (offset != that.offset) return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result = (int)(offset ^ (offset >> 32));
            result = 31 * result + (int)(length ^ (length >> 32));
            return result;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", offset, length);
        }
    }
}
