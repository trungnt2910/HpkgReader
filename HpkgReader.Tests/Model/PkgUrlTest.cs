/*
 * Copyright 2015-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Model;
using NUnit.Framework;
using System;

namespace HpkgReader.Tests.Model
{
    public class PkgUrlTest
    {
        /// <summary>
        /// Checks that if there is an empty string put in that it will throw.
        /// </summary>
        [Test]
        public void TestConstruct_Bad()
        {
            Assert.That(() =>
            {
                new PkgUrl("", PkgUrlType.HOMEPAGE);
            }, Throws.Exception);
        }

        [Test]
        public void TestConstruct_NakedUrl()
        {
            PkgUrl pkgUrl = new PkgUrl("http://www.haiku-os.org", PkgUrlType.HOMEPAGE);
            Assert.That(pkgUrl.Url == "http://www.haiku-os.org");
            Assert.That(pkgUrl.Name == null);
        }

        [Test]
        public void TestConstruct_NameWithUrl()
        {
            PkgUrl pkgUrl = new PkgUrl("Waiheke Island <http://www.haiku-os.org>", PkgUrlType.HOMEPAGE);
            Assert.That(pkgUrl.Url == "http://www.haiku-os.org");
            Assert.That(pkgUrl.Name == "Waiheke Island");
        }

        [Test]
        public void TestConstruct_EmptyNameWithUrl()
        {
            PkgUrl pkgUrl = new PkgUrl("<http://www.haiku-os.org>", PkgUrlType.HOMEPAGE);
            Assert.That(pkgUrl.Url == "http://www.haiku-os.org");
            Assert.That(pkgUrl.Name == null);
        }

        [Test]
        public void TestConstruct_NameWithUrlNoSpaceSeparator()
        {
            PkgUrl pkgUrl = new PkgUrl("Iceland<http://www.haiku-os.org>", PkgUrlType.HOMEPAGE);
            Assert.That(pkgUrl.Url == "http://www.haiku-os.org");
            Assert.That(pkgUrl.Name == "Iceland");
        }
    }
}
