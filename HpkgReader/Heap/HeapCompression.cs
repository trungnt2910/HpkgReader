/*
 * Copyright 2013-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using System;

namespace HpkgReader.Heap
{
    public enum HeapCompression
    {
        NONE = 0,
        ZLIB = 1
    }

    public static class HeapCompressionCompat
    {
        public static HeapCompression GetByNumericValue(int value)
        {
            if (!Enum.IsDefined(typeof(HeapCompression), value))
            {
                throw new HpkException("unknown compression numeric value [" + value + "]");
            }
            return (HeapCompression)value;
        }
    }
}