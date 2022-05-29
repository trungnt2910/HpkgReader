/*
 * Copyright 2021, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

namespace HpkgReader.Model
{
    /// <summary>
    /// <para>The attribute type <see cref="AttributeId.FILE_ATTRIBUTE"/> has a value
    /// which is a string that defines what sort of attribute it is.</para>
    /// </summary>
    public class FileAttributesValues
    {
        public readonly FileAttributesValues BEOS_ICON = new FileAttributesValues("BEOS:ICON");

        private readonly string attributeValue;

        private FileAttributesValues(string attributeValue)
        {
            this.attributeValue = attributeValue;
        }

        public string AttributeValue => attributeValue;
    }
}
