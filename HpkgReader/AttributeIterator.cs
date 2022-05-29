/*
 * Copyright 2018-2019, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using HpkgReader.Compat;
using HpkgReader.Heap;
using HpkgReader.Model;

using Attribute = HpkgReader.Model.Attribute;

namespace HpkgReader
{
    /// <summary>
    /// <para>This object is able to provide an iterator through all of the attributes at a given offset in the chunks. The
    /// chunk data is supplied through an instance of <see cref="AttributeContext"/>. It will work through all of the 
    /// attributes serially and will also process all of the child-attributes as well. The iteration process means that
    /// less in-memory data is required to process a relatively long list of attributes.</para>
    ///  
    /// <para>Use the method <see cref="HasNext"/> to find out if there is another attribute to read and <see cref="Next"/> in 
    /// order to obtain the next attribute.</para>
    /// 
    /// <para>Note that this does not actually implement <see cref="Iterator"/> because it needs to throw Hpk exceptions
    /// which would mean that it were not compliant with the @{link Iterator} interface.</para>
    /// </summary>
    public class AttributeIterator
    {

        private const int ATTRIBUTE_TYPE_INVALID = 0;
        private const int ATTRIBUTE_TYPE_INT = 1;
        private const int ATTRIBUTE_TYPE_UINT = 2;
        private const int ATTRIBUTE_TYPE_STRING = 3;
        private const int ATTRIBUTE_TYPE_RAW = 4;

        private const int ATTRIBUTE_ENCODING_INT_8_BIT = 0;
        private const int ATTRIBUTE_ENCODING_INT_16_BIT = 1;
        private const int ATTRIBUTE_ENCODING_INT_32_BIT = 2;
        private const int ATTRIBUTE_ENCODING_INT_64_BIT = 3;

        private const int ATTRIBUTE_ENCODING_STRING_INLINE = 0;
        private const int ATTRIBUTE_ENCODING_STRING_TABLE = 1;

        private const int ATTRIBUTE_ENCODING_RAW_INLINE = 0;
        private const int ATTRIBUTE_ENCODING_RAW_HEAP = 1;

        private long offset;

        private readonly AttributeContext context;

        private BigInteger? nextTag = null;

        public AttributeIterator(AttributeContext context, long offset)
        {
            Preconditions.CheckNotNull(context);
            Preconditions.CheckState(offset >= 0 && offset < int.MaxValue);

            this.offset = offset;
            this.context = context;
        }

        public AttributeContext Context => context;

        public long Offset => offset;

        /// <summary>
        /// This method allows the caller to discover if there is another attribute to get off the iterator.
        /// </summary>
        /// <returns></returns>
        public bool HasNext()
        {
            return 0 != GetNextTag().Sign;
        }

        /// <summary>
        /// This method will return the next <see cref="Attribute"/>. If there is not another value to return then
        /// this method will return null. It will throw an instance of @{link HpkException} in any situation in which
        /// it is not able to parse the data or chunks such that it is not able to read the next attribute.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HpkException"></exception>
        public Attribute Next()
        {

            Attribute result = null;

            // first, the LEB128 has to be read in which is the 'tag' defining what sort of attribute this is that
            // we are dealing with.

            BigInteger tag = GetNextTag();

            // if we encounter 0 tag then we know that we have finished the list.

            if (0 != tag.Sign)
            {
                int encoding = DeriveAttributeTagEncoding(tag);
                int id = DeriveAttributeTagId(tag);

                if (id < 0 || id >= AttributeId.Values.Length)
                {
                    throw new HpkException("illegal id; " + id);
                }
                AttributeId attributeId = AttributeId.Values[id];
                result = ReadAttributeByTagType(DeriveAttributeTagType(tag), encoding, attributeId);
                EnsureAttributeType(result);

                if (result.AttributeId.AttributeType != result.AttributeType)
                {
                    throw new HpkException(string.Format(
                            "mismatch in attribute type for id {0}; expecting {1}, but got {2}",
                            result.AttributeId.Name,
                            result.AttributeId.AttributeType,
                            result.AttributeType));
                }

                // possibly there are child attributes after this attribute; if this is the
                // case then open-up a new iterator to work across those and load them in.
                if (DeriveAttributeTagHasChildAttributes(tag))
                {
                    result.SetChildAttributes(ReadChildAttributes());
                }

                nextTag = null;
            }

            return result;
        }

        private List<Attribute> ReadChildAttributes()
        {
            var children = new List<Attribute>();
            AttributeIterator childAttributeIterator = new AttributeIterator(context, offset);

            while (childAttributeIterator.HasNext())
            {
                children.Add(childAttributeIterator.Next());
            }

            offset = childAttributeIterator.Offset;
            return children;
        }

        /**
         * each attribute id has a type associated with it; now check that the attribute matches
         * its intended type.
         */

        private void EnsureAttributeType(Attribute attribute)
        {
            if (attribute.AttributeId.AttributeType != attribute.AttributeType)
            {
                throw new HpkException(string.Format(
                        "mismatch in attribute type for id {0}; expecting {1}, but got {2}",
                        attribute.AttributeId.Name,
                        attribute.AttributeId.AttributeType,
                        attribute.AttributeType));
            }
        }

        private Attribute ReadAttributeByTagType(int tagType, int encoding, AttributeId attributeId)
        {
            switch (tagType)
            {
                case ATTRIBUTE_TYPE_INVALID:
                    throw new HpkException("an invalid attribute tag type has been encountered");
                case ATTRIBUTE_TYPE_INT:
                    return new IntAttribute(attributeId, BigIntegerCompat.Construct(ReadBufferForInt(encoding)));
                case ATTRIBUTE_TYPE_UINT:
                    return new IntAttribute(attributeId, BigIntegerCompat.Construct(1, ReadBufferForInt(encoding)));
                case ATTRIBUTE_TYPE_STRING:
                    return ReadString(encoding, attributeId);
                case ATTRIBUTE_TYPE_RAW:
                    return ReadRaw(encoding, attributeId);
                default:
                    throw new HpkException("unable to read the tag type [" + tagType + "]");

            }
        }

        private byte[] ReadBufferForInt(int encoding)
        {
            EnsureValidEncodingForInt(encoding);
            int bytesToRead = 1 << encoding;
            byte[] buffer = new byte[bytesToRead];
            context.HeapReader.ReadHeap(buffer, 0, new HeapCoordinates(offset, bytesToRead));
            offset += bytesToRead;

            return buffer;
        }

        private Attribute ReadString(int encoding, AttributeId attributeId)
        {
            switch (encoding)
            {
                case ATTRIBUTE_ENCODING_STRING_INLINE:
                    return ReadStringInline(attributeId);
                case ATTRIBUTE_ENCODING_STRING_TABLE:
                    return ReadStringTable(attributeId);
                default:
                    throw new HpkException("unknown string encoding; " + encoding);
            }
        }

        private Attribute ReadStringTable(AttributeId attributeId)
        {
            BigInteger index = ReadUnsignedLeb128();

            if (index.CompareTo(int.MaxValue) > 0)
            {
                throw new InvalidOperationException("the string table index is preposterously large");
            }

            return new StringTableRefAttribute(attributeId, (int)index);
        }

        private Attribute ReadStringInline(AttributeId attributeId)
        {
            var assembly = new MemoryStream();

            while (true)
            {
                int b = context.HeapReader.ReadHeap(offset);
                offset++;

                if (0 != b)
                {
                    assembly.WriteByte(unchecked((byte)b));
                }
                else
                {
                    return new StringInlineAttribute(
                            attributeId,
                            Encoding.UTF8.GetString(assembly.ToArray()));
                }
            }
        }

        private Attribute ReadRaw(int encoding, AttributeId attributeId)
        {
            switch (encoding)
            {
                case ATTRIBUTE_ENCODING_RAW_INLINE:
                    return ReadRawInline(attributeId);
                case ATTRIBUTE_ENCODING_RAW_HEAP:
                    return ReadRawHeap(attributeId);
                default:
                    throw new HpkException("unknown raw encoding; " + encoding);
            }
        }

        private Attribute ReadRawInline(AttributeId attributeId)
        {
            BigInteger length = ReadUnsignedLeb128();

            if (length.CompareTo(int.MaxValue) > 0)
            {
                throw new HpkException("the length of the inline data is too large");
            }

            byte[] buffer = new byte[(int)length];
            context.HeapReader.ReadHeap(buffer, 0, new HeapCoordinates(offset, (int)length));
            offset += (int)length;

            return new RawInlineAttribute(attributeId, buffer);
        }

        private Attribute ReadRawHeap(AttributeId attributeId)
        {
            BigInteger rawLength = ReadUnsignedLeb128();
            BigInteger rawOffset = ReadUnsignedLeb128();

            if (rawLength.CompareTo(int.MaxValue) > 0)
            {
                throw new HpkException("the length of the heap data is too large");
            }

            if (rawOffset.CompareTo(int.MaxValue) > 0)
            {
                throw new HpkException("the offset of the heap data is too large");
            }

            return new RawHeapAttribute(
                    attributeId,
                    new HeapCoordinates(
                            (long)rawOffset,
                            (long)rawLength));
        }


        private int DeriveAttributeTagType(BigInteger tag)
        {
            return (int)(((tag - 1) >> 7) & 0x7L);
        }

        private int DeriveAttributeTagId(BigInteger tag)
        {
            return (int)((tag - 1) & 0x7FL);
        }

        private int DeriveAttributeTagEncoding(BigInteger tag)
        {
            return (int)(((tag - 1) >> 11) & 3L);
        }

        private bool DeriveAttributeTagHasChildAttributes(BigInteger tag)
        {
            return 0 != (int)(((tag - 1) >> 10) & 1);
        }

        private BigInteger GetNextTag()
        {
            if (null == nextTag)
            {
                nextTag = ReadUnsignedLeb128();
            }

            return (BigInteger)nextTag;
        }

        private BigInteger ReadUnsignedLeb128()
        {
            BigInteger result = BigInteger.Zero;
            int shift = 0;

            while (true)
            {
                int b = context.HeapReader.ReadHeap(offset);
                offset++;

                result |= new BigInteger(b & 0x7f) << shift;

                if (0 == (b & 0x80))
                {
                    return result;
                }

                shift += 7;
            }
        }

        private void EnsureValidEncodingForInt(int encoding)
        {
            switch (encoding)
            {
                case ATTRIBUTE_ENCODING_INT_8_BIT:
                case ATTRIBUTE_ENCODING_INT_16_BIT:
                case ATTRIBUTE_ENCODING_INT_32_BIT:
                case ATTRIBUTE_ENCODING_INT_64_BIT:
                    break;
                default:
                    throw new InvalidOperationException("unknown encoding on a signed integer");
            }
        }

    }
}