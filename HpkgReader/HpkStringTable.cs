/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using HpkgReader.Heap;

namespace HpkgReader
{
    /// <summary>
    /// The HPK* file format may contain a table of commonly used strings in a table.  This object will represent
    /// those strings and will lazily load them from the heap as necessary.
    /// </summary>
    public class HpkStringTable : StringTable
    {

        private readonly HpkHeapReader heapReader;

        private readonly long expectedCount;

        private readonly long heapLength;

        private readonly long heapOffset;

        private string[] values = null;

        public HpkStringTable(
                HpkHeapReader heapReader,
                long heapOffset,
                long heapLength,
                long expectedCount)
            : base()
        {

            Preconditions.CheckNotNull(heapReader);
            Preconditions.CheckState(heapOffset >= 0 && heapOffset < int.MaxValue);
            Preconditions.CheckState(heapLength >= 0 && heapLength < int.MaxValue);
            Preconditions.CheckState(expectedCount >= 0 && expectedCount < int.MaxValue);

            this.heapReader = heapReader;
            this.expectedCount = expectedCount;
            this.heapOffset = heapOffset;
            this.heapLength = heapLength;

        }

        // TODO; could avoid the big read into a buffer by reading the heap byte by byte or with a buffer.
        private string[] ReadStrings()
        {
            string[] result = new string[(int)expectedCount];
            byte[] stringsDataBuffer = new byte[(int)heapLength];

            heapReader.ReadHeap(stringsDataBuffer, 0,
                    new HeapCoordinates(heapOffset, heapLength));

            // now work through the data and load them into the strings.

            int stringIndex = 0;
            int offset = 0;

            while (offset < stringsDataBuffer.Length)
            {

                if (0 == stringsDataBuffer[offset])
                {
                    if (stringIndex != result.Length)
                    {
                        throw new HpkException(string.Format("expected to read %d package strings from the strings table, but actually found {0}", expectedCount, stringIndex));
                    }

                    return result;
                }

                if (stringIndex >= expectedCount)
                {
                    throw new HpkException("have already read all of the strings from the string table, but have not exhausted the string table data");
                }

                int start = offset;

                while (offset < stringsDataBuffer.Length && 0 != stringsDataBuffer[offset])
                {
                    offset++;
                }

                if (offset < stringsDataBuffer.Length)
                {
                    result[stringIndex] = StringCompat.Construct(stringsDataBuffer, start, offset - start, Charsets.UTF_8);
                    stringIndex++;
                    offset++;
                }

            }

            throw new HpkException("expected to find the null-terminator for the list of strings, but was not able to find one; did read " + stringIndex + " of " + expectedCount);
        }

        private string[] GetStrings()
        {
            if (null == values)
            {
                if (0 == heapLength)
                {
                    values = new string[] { };
                }
                else
                {
                    values = ReadStrings();
                }
            }

            return values;
        }

        public override string GetString(int index)
        {
            return GetStrings()[index];
        }
    }
}
