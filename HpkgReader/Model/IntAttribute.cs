/*
 * Copyright 2018-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using System.Numerics;

namespace HpkgReader.Model
{
    /// <summary>
    /// This attribute is an integral numeric value. Note that the format specifies either a signed or unsigned value,
    /// but this concrete subclass of @{link Attribute} serves for both the signed and unsigned cases.
    /// </summary>
    public class IntAttribute: Attribute
    {
        private readonly BigInteger numericValue;

        public IntAttribute(AttributeId attributeId, BigInteger numericValue)
            : base(attributeId)
        {
            this.numericValue = numericValue;
        }

        public override object GetValue(AttributeContext context)
        {
            return numericValue;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            IntAttribute that = (IntAttribute)o;

            if (!numericValue.Equals(that.numericValue)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return numericValue.GetHashCode();
        }

        public override AttributeType AttributeType => AttributeType.INT;

        public override string ToString()
        {
            return string.Format("{0} : {1}", base.ToString(), numericValue.ToString());
        }
    }
}