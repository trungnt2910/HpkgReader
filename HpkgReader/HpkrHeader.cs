/*
 * Copyright 2013, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Heap;

namespace HpkgReader
{
    public class HpkrHeader
    {
        private long headerSize;
        private int version;
        private long totalSize;
        private int minorVersion;

        // heap
        private HeapCompression heapCompression;
        private long heapChunkSize;
        private long heapSizeCompressed;
        private long heapSizeUncompressed;

        // repository info section
        private long infoLength;

        // package attributes section
        private long packagesLength;
        private long packagesStringsLength;
        private long packagesStringsCount;

        public long HeaderSize
        {
            get => headerSize;
            set => headerSize = value;
        }

        public int Version
        {
            get => version;
            set => version = value;
        }

        public long TotalSize
        {
            get => totalSize;
            set => totalSize = value;
        }

        public int MinorVersion
        {
            get => minorVersion;
            set => minorVersion = value;
        }

        public HeapCompression HeapCompression 
        {
            get => heapCompression; 
            set => heapCompression = value;
        }

        public long HeapChunkSize
        {
            get => heapChunkSize;
            set => heapChunkSize = value;
        }

        public long HeapSizeCompressed
        {
            get => heapSizeCompressed;
            set => heapSizeCompressed = value;
        }

        public long HeapSizeUncompressed
        {
            get => heapSizeUncompressed;
            set => heapSizeUncompressed = value;
        }

        public long InfoLength
        {
            get => infoLength;
            set => infoLength = value;
        }

        public long PackagesLength
        {
            get => packagesLength;
            set => packagesLength = value;
        }

        public long PackagesStringsLength
        {
            get => packagesStringsLength;
            set => packagesStringsLength = value;
        }

        public long PackagesStringsCount
        {
            get => packagesStringsCount;
            set => packagesStringsCount = value;
        }
    }
}
