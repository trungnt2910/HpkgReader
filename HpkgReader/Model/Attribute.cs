/*
 * Copyright 2018-2021, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HpkgReader.Model
{
    /// <summary>
    /// This is the superclass of the different types (data types) of attributes.
    /// </summary>
    public abstract class Attribute
    {

        private readonly AttributeId attributeId;

        private List<Attribute> childAttributes = new List<Attribute>();

        public Attribute(AttributeId attributeId)
        {
            this.attributeId = attributeId;
        }

        public AttributeId AttributeId => attributeId;

        public abstract AttributeType AttributeType { get; }

        public abstract object GetValue(AttributeContext context);

        public T GetValue<T>(AttributeContext context)
        {
            return (T)GetValue(context);
        }

        public void SetChildAttributes(List<Attribute> value)
        {
            childAttributes = (null == value)
                    ? new List<Attribute>()
                    : new List<Attribute>(value);
        }

        public bool HasChildAttributes()
        {
            return childAttributes.Count != 0;
        }

        public List<Attribute> GetChildAttributes()
        {
            return childAttributes;
        }

        public List<Attribute> GetChildAttributes(AttributeId attributeId)
        {
            Preconditions.CheckNotNull(attributeId);
            return childAttributes.Where(a => a.AttributeId == attributeId).ToList();
        }

        public Attribute TryGetChildAttribute(AttributeId attributeId)
        {
            Preconditions.CheckNotNull(attributeId);
            return childAttributes.FirstOrDefault(a => a.AttributeId == attributeId);
        }

        public Attribute GetChildAttribute(AttributeId attributeId)
        {
            return TryGetChildAttribute(attributeId) ?? throw new ArgumentException("unable to find the attribute [" + attributeId.Name + "]");
        }

        public override string ToString()
        {
            return AttributeId.Name + " : " + AttributeType.ToString();
        }
    }
}