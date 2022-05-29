/*
 * Copyright 2018-2021, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using HpkgReader.Heap;
using System.IO;

namespace HpkgReader.Model
{
    /// <summary>
    /// This type of attribute refers to raw data. It uses coordinates into the heap to provide a source for the
    /// data.
    /// </summary>
    public class RawHeapAttribute : RawAttribute
    {

        private readonly HeapCoordinates heapCoordinates;

        public RawHeapAttribute(AttributeId attributeId, HeapCoordinates heapCoordinates)
            : base(attributeId)
        {
            Preconditions.CheckNotNull(heapCoordinates);
            this.heapCoordinates = heapCoordinates;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            RawHeapAttribute that = (RawHeapAttribute)o;

            if (!heapCoordinates.Equals(that.heapCoordinates)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return heapCoordinates.GetHashCode();
        }

        public override object GetValue(AttributeContext context)
        {
            return new HeapByteSource(context.HeapReader, heapCoordinates);
        }

        public override AttributeType AttributeType => AttributeType.RAW;

        public override string ToString()
        {
            return string.Format("{0} : @{1}", base.ToString(), heapCoordinates.ToString());
        }

        public class HeapByteSource : ByteSource
        {

            private readonly HeapReader heapReader;
            private readonly HeapCoordinates heapCoordinates;

            public HeapByteSource(HeapReader heapReader, HeapCoordinates heapCoordinates)
            {
                this.heapCoordinates = heapCoordinates;
                this.heapReader = heapReader;
            }

            public override Stream OpenStream()
            {
                return new HeapInputStream(heapReader, heapCoordinates);
            }

            public override long? SizeIfKnown()
            {
                return heapCoordinates.Length;
            }

            public HeapCoordinates HeapCoordinates => heapCoordinates;
        }

    }

}