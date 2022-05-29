/*
 * Copyright 2018-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

package org.haiku.pkg.output;

import com.google.common.base.Preconditions;
import com.google.common.io.ByteSource;
import org.haiku.pkg.AttributeContext;
import org.haiku.pkg.HpkException;
import org.haiku.pkg.heap.HeapCoordinates;
import org.haiku.pkg.model.Attribute;
import org.haiku.pkg.AttributeIterator;
import org.haiku.pkg.model.RawHeapAttribute;

import java.io.FilterWriter;
import java.io.IOException;
import java.io.Writer;

/**
 * <p>This is a writer in the sense that it writes a human-readable dump of the attributes to a {@link Writer}.  This
 * enables debug or diagnostic output to be written to, for example, a file or standard output.  It will write the
 * attributes in an indented tree structure.</p>
 */

public class AttributeWriter extends FilterWriter {

    public AttributeWriter(Writer writer) {
        super(writer);
    }

    private void write(int indent, AttributeContext context, Attribute attribute) throws IOException {
        Preconditions.checkNotNull(context);
        Preconditions.checkNotNull(attribute);
        Preconditions.checkState(indent >= 0);

        for (int i = 0; i < indent; i++) {
            write(' ');
        }

        write(attribute.getAttributeId().getName());
        write(" : ");
        write(attribute.getAttributeType().name());
        write(" : ");

        try {
            switch (attribute.getAttributeType()) {
                case RAW:
                    ByteSource byteSource = (ByteSource) attribute.getValue(context);
                    write(String.format("%d bytes", byteSource.size()));
                    if (byteSource instanceof RawHeapAttribute.HeapByteSource) {
                        HeapCoordinates coordinates = ((RawHeapAttribute.HeapByteSource) byteSource).getHeapCoordinates();
                        write(String.format(" {off:%d, len:%d}", coordinates.getOffset(), coordinates.getLength()));
                    }
                    break;
                case INT:
                case STRING:
                    write(attribute.getValue(context).toString());
                    break;
                default:
                    write("???");
                    break;
            }
        }
        catch (HpkException e) {
            throw new IOException("unable to process an attribute '" + attribute + "'",e);
        }

        write("\n");

        if (attribute.hasChildAttributes()) {
            for (Attribute childAttribute : attribute.getChildAttributes()) {
                write(indent + 2, context, childAttribute);
            }
        }
    }

    public void write(AttributeContext context, Attribute attribute) throws IOException {
        Preconditions.checkNotNull(context);
        Preconditions.checkNotNull(attribute);
        write(0, context, attribute);
    }

    public void write(AttributeIterator attributeIterator) throws IOException {
        Preconditions.checkNotNull(attributeIterator);

        while (attributeIterator.hasNext()) {
            try {
                write(attributeIterator.getContext(), attributeIterator.next());
            }
            catch (HpkException e) {
                throw new IOException("unable to get the next attribute on the interator", e);
            }
        }
    }

}
