/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

namespace HpkgReader.Heap
{
    /// <summary>
    /// This is an interface for classes that are able to provide data from a block of memory referred to as "the heap".
    /// Concrete sub-classes are able to provide specific implementations that can read from different on-disk files to
    /// provide access to a heap.
    /// </summary>
    public abstract class HeapReader
    {
        /// <summary>
        /// This method reads from the heap (possibly across chunks) the data described in the coordinates attribute. It
        /// writes those bytes into the supplied buffer at the offset supplied.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bufferOffset"></param>
        /// <param name="coordinates"></param>
        public abstract void ReadHeap(byte[] buffer, int bufferOffset, HeapCoordinates coordinates);

        /// <summary>
        /// This method reads a single byte of the heap at the given offset.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public abstract int ReadHeap(long offset);
    }
}
