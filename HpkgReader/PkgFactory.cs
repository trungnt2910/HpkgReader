/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using HpkgReader.Extensions;
using HpkgReader.Model;
using System.Numerics;
using System.Collections.Generic;
using System;
using System.Linq;

using Attribute = HpkgReader.Model.Attribute;

namespace HpkgReader
{
    /// <summary>
    /// <para>This object is algorithm that is able to convert a top level package attribute into a modelled package object
    /// that can more easily represent the package; essentially converting the low-level attributes into a higher-level
    /// package model object.</para>
    /// </summary>
    public class PkgFactory
    {

        public Pkg CreatePackage(
                AttributeContext attributeContext,
                Attribute attribute)
        {

            Preconditions.CheckNotNull(attribute);
            Preconditions.CheckNotNull(attributeContext);
            Preconditions.CheckState(attribute.AttributeId == AttributeId.PACKAGE);

            try
            {
                return new Pkg(
                        GetStringAttributeValue(attributeContext, attribute, AttributeId.PACKAGE_NAME),
                        CreateVersion(attributeContext, attribute.GetChildAttribute(AttributeId.PACKAGE_VERSION_MAJOR)),
                        CreateArchitecture(attributeContext, attribute.GetChildAttribute(AttributeId.PACKAGE_ARCHITECTURE)),
                        GetStringAttributeValue(attributeContext, attribute, AttributeId.PACKAGE_VENDOR),
                        GetChildAttributesAsStrings(attributeContext, attribute.GetChildAttributes(AttributeId.PACKAGE_COPYRIGHT)),
                        GetChildAttributesAsStrings(attributeContext, attribute.GetChildAttributes(AttributeId.PACKAGE_LICENSE)),
                        GetStringAttributeValue(attributeContext, attribute, AttributeId.PACKAGE_SUMMARY),
                        GetStringAttributeValue(attributeContext, attribute, AttributeId.PACKAGE_DESCRIPTION),
                        TryCreateHomePagePkgUrl(attributeContext, attribute));
            }
            catch (HpkException he)
            {
                throw new PkgException("unable to create a package owing to a problem with the hpk packaging", he);
            }
        }

        private string TryGetStringAttributeValue(
                AttributeContext attributeContext,
                Attribute attribute,
                AttributeId attributeId)
        {
            return attribute.TryGetChildAttribute(attributeId)
                ?.GetValue(attributeContext)
                as string;
        }

        private string GetStringAttributeValue(
                AttributeContext attributeContext,
                Attribute attribute,
                AttributeId attributeId)
        {
            return TryGetStringAttributeValue(attributeContext, attribute, attributeId)
                    ?? throw new PkgException(string.Format("the {0} attribute must be present", attributeId.Name));
        }

        private PkgVersion CreateVersion(
                AttributeContext attributeContext,
                Attribute attribute)
        {

            Preconditions.CheckNotNull(attribute);
            Preconditions.CheckNotNull(attributeContext);
            Preconditions.CheckState(AttributeId.PACKAGE_VERSION_MAJOR == attribute.AttributeId);

            return new PkgVersion(
                    (string)attribute.GetValue(attributeContext),
                    TryGetStringAttributeValue(attributeContext, attribute, AttributeId.PACKAGE_VERSION_MINOR),
                    TryGetStringAttributeValue(attributeContext, attribute, AttributeId.PACKAGE_VERSION_MICRO),
                    TryGetStringAttributeValue(attributeContext, attribute, AttributeId.PACKAGE_VERSION_PRE_RELEASE),
                    attribute.TryGetChildAttribute(AttributeId.PACKAGE_VERSION_REVISION)
                        ?.GetValue<BigInteger?>(attributeContext)
                        ?.IntValue()
            );
        }

        private PkgArchitecture CreateArchitecture(
                AttributeContext attributeContext,
                Attribute attribute)
        {

            Preconditions.CheckNotNull(attribute);
            Preconditions.CheckNotNull(attributeContext);
            Preconditions.CheckState(AttributeId.PACKAGE_ARCHITECTURE == attribute.AttributeId);

            int value = (int)(BigInteger)attribute.GetValue(attributeContext);
            return (PkgArchitecture)Enum.ToObject(typeof(PkgArchitecture), value);
        }

        private List<string> GetChildAttributesAsStrings(
                AttributeContext attributeContext,
                List<Attribute> attributes)
        {
            return attributes.Select(a => a.GetValue(attributeContext).ToString()).ToList();
        }

        private PkgUrl TryCreateHomePagePkgUrl(
                AttributeContext attributeContext,
                Attribute attribute)
        {
            return TryGetStringAttributeValue(attributeContext, attribute, AttributeId.PACKAGE_URL)
                .Map(v => new PkgUrl(v, PkgUrlType.HOMEPAGE));
        }
    }
}
