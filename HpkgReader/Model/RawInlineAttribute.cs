/*
 * Copyright 2018-2021, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using System.Linq;

namespace HpkgReader.Model
{
    public class RawInlineAttribute : RawAttribute
    {
        private readonly byte[] rawValue;

        public RawInlineAttribute(AttributeId attributeId, byte[] rawValue)
            : base(attributeId)
        {
            Preconditions.CheckNotNull(rawValue);
            this.rawValue = rawValue;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            RawInlineAttribute that = (RawInlineAttribute)o;

            if (!Enumerable.SequenceEqual(rawValue, that.rawValue)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Arrays.HashCode(rawValue);
        }

        public override object GetValue(AttributeContext context)
        {
            return ByteSource.Wrap(rawValue);
        }

        public override AttributeType AttributeType => AttributeType.RAW;

        public override string ToString()
        {
            return string.Format("{0} : {1} b", base.ToString(), rawValue.Length);
        }
    }
}
