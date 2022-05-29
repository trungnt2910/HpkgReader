/*
 * Copyright 2013, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Heap;

namespace HpkgReader
{
    /// <summary>
    /// This object carries around pointers to other data structures and model objects that are required to
    /// support the processing of attributes.
    /// </summary>
    public class AttributeContext
    {
        public HeapReader HeapReader { get; set; }

        public StringTable StringTable { get; set; }
    }
}
