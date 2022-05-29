/*
 * Copyright 2013, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;

namespace HpkgReader.Model
{
    /// <summary>
    /// This type of attribute references a string from an instance of <see cref="HpkStringTable"/>
    /// which is typically obtained from an instance of <see cref="AttributeContext"/>.
    /// </summary>
    public class StringTableRefAttribute : StringAttribute
    {
        private readonly int index;

        public StringTableRefAttribute(AttributeId attributeId, int index)
            : base(attributeId)
        {
            Preconditions.CheckArgument(index >= 0, "bad index");
            this.index = index;
        }

        public override object GetValue(AttributeContext context)
        {
            return context.StringTable.GetString(index);
        }

        public override AttributeType AttributeType => AttributeType.STRING;

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            StringTableRefAttribute that = (StringTableRefAttribute)o;

            if (index != that.index)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return index;
        }

        public override string ToString()
        {
            return string.Format("{0} : @{1}", base.ToString(), index);
        }
    }
}
