/*
 * Copyright 2018-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;

namespace HpkgReader.Model
{
    /// <summary>
    /// This type of attribute is a string. The string is supplied in the stream of attributes so this attribute will
    /// carry the string.
    /// </summary>
    public class StringInlineAttribute : StringAttribute
	{

		private readonly string stringValue;

		public StringInlineAttribute(AttributeId attributeId, string stringValue)
			: base(attributeId)
		{
			Preconditions.CheckNotNull(stringValue);
			this.stringValue = stringValue;
		}

		public override object GetValue(AttributeContext context)
		{
			return stringValue;
		}

		public override AttributeType AttributeType => AttributeType.STRING;

		public override bool Equals(object o)
		{
			if (this == o) return true;
			if (o == null || GetType() != o.GetType()) return false;

			StringInlineAttribute that = (StringInlineAttribute)o;

			if (!stringValue.Equals(that.stringValue)) return false;

			return true;
		}

		public override int GetHashCode()
		{
			return stringValue.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("{0} : {1}", base.ToString(), stringValue);
		}
	}
}
