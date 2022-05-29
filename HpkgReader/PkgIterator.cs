/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using HpkgReader.Model;

namespace HpkgReader
{
    /// <summary>
    /// <para>This object will wrap an attribute iterator to be able to generate a series of <see cref="Pkg"/> objects that
    /// model a package in the Haiku package management system.</para>
    /// </summary>
    public class PkgIterator
    {

        private readonly AttributeIterator attributeIterator;
        private readonly PkgFactory pkgFactory;

        public PkgIterator(AttributeIterator attributeIterator)
            : this(attributeIterator, new PkgFactory())
        {
        }

        private PkgIterator(AttributeIterator attributeIterator, PkgFactory pkgFactory)
            : base()
        {
            Preconditions.CheckNotNull(attributeIterator);
            this.attributeIterator = attributeIterator;
            this.pkgFactory = pkgFactory;
        }

        /// <summary>
        /// This method will return true if there are more packages to be obtained from the attributes iterator.
        /// </summary>
        /// <returns></returns>
        public bool HasNext()
        {
            return attributeIterator.HasNext();
        }

        /// <summary>
        /// <para>This method will return the next package from the attribute iterator supplied.</para>
        /// </summary>
        /// <returns>The return value is the next package from the list of attributes.</returns>
        /// <exception cref="PkgException">when there is a problem obtaining the next package from the attributes.</exception>
        /// <exception cref="HpkException">when there is a problem obtaining the next attributes.</exception>
        public Pkg Next()
        {
            Attribute attribute = attributeIterator.Next();

            if (null != attribute)
            {
                return pkgFactory.CreatePackage(attributeIterator.Context, attribute);
            }

            return null;
        }
    }
}
