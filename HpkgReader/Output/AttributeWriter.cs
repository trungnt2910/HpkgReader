/*
 * Copyright 2018-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using HpkgReader.Heap;
using HpkgReader.Model;
using System.IO;

namespace HpkgReader.Output
{
    /// <summary>
    /// <para>This is a writer in the sense that it writes a human-readable dump of the attributes to a <see cref="TextWriter"/>.  This
    /// enables debug or diagnostic output to be written to, for example, a file or standard output.  It will Write the
    /// attributes in an indented tree structure.</para>
    /// </summary>
    public class AttributeWriter : FilterWriter
    {
        public AttributeWriter(TextWriter writer)
            : base(writer)
        {
        }

        private void Write(int indent, AttributeContext context, Attribute attribute)
        {
            Preconditions.CheckNotNull(context);
            Preconditions.CheckNotNull(attribute);
            Preconditions.CheckState(indent >= 0);

            for (int i = 0; i < indent; i++)
            {
                Write(' ');
            }

            Write(attribute.AttributeId.Name);
            Write(" : ");
            Write(attribute.AttributeType.ToString());
            Write(" : ");

            try
            {
                switch (attribute.AttributeType)
                {
                    case AttributeType.RAW:
                        ByteSource byteSource = (ByteSource)attribute.GetValue(context);
                        Write(string.Format("{0} bytes", byteSource.Size()));
                        if (byteSource is RawHeapAttribute.HeapByteSource)
                        {
                            HeapCoordinates coordinates = ((RawHeapAttribute.HeapByteSource)byteSource).HeapCoordinates;
                            Write(string.Format(" {off:{0}, len:{1}", coordinates.Offset, coordinates.Length));
                        }
                        break;
                    case AttributeType.INT:
                    case AttributeType.STRING:
                        Write(attribute.GetValue(context).ToString());
                        break;
                    default:
                        Write("???");
                        break;
                }
            }
            catch (HpkException e)
            {
                throw new IOException("unable to process an attribute '" + attribute + "'", e);
            }

            Write("\n");

            if (attribute.HasChildAttributes())
            {
                foreach (Attribute childAttribute in attribute.GetChildAttributes())
                {
                    Write(indent + 2, context, childAttribute);
                }
            }
        }

        public void Write(AttributeContext context, Attribute attribute)
        {
            Preconditions.CheckNotNull(context);
            Preconditions.CheckNotNull(attribute);
            Write(0, context, attribute);
        }

        public void Write(AttributeIterator attributeIterator)
        {
            Preconditions.CheckNotNull(attributeIterator);

            while (attributeIterator.HasNext())
            {
                try
                {
                    Write(attributeIterator.Context, attributeIterator.Next());
                }
                catch (HpkException e)
                {
                    throw new IOException("unable to get the next attribute on the interator", e);
                }
            }
        }
    }
}
