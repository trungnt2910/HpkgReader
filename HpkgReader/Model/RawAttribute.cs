/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

namespace HpkgReader.Model
{
	public abstract class RawAttribute: Attribute
	{
		protected RawAttribute(AttributeId attributeId)
			: base(attributeId)
		{
		}
	}
}
