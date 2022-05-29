/*
 * Copyright 2018-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using NUnit.Framework;
using HpkgReader.Model;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace HpkgReader
{
	/// <summary>
	/// <para>This is a very simplistic test that checks that attributes are able to be converted into package DTO model 
	/// objects correctly.</para>
	/// </summary>
	public class PkgFactoryTest
	{
		private Attribute CreateTestPackageAttributes()
		{

			StringInlineAttribute majorVersionA = new StringInlineAttribute(AttributeId.PACKAGE_VERSION_MAJOR, "6");
			majorVersionA.SetChildAttributes(new List<Attribute>()
			{
				new StringInlineAttribute(AttributeId.PACKAGE_VERSION_MINOR, "32"),
				new StringInlineAttribute(AttributeId.PACKAGE_VERSION_MICRO, "9"),
				new StringInlineAttribute(AttributeId.PACKAGE_VERSION_PRE_RELEASE, "beta"),
				new IntAttribute(AttributeId.PACKAGE_VERSION_REVISION, new BigInteger(8))
			});

			StringInlineAttribute topA = new StringInlineAttribute(AttributeId.PACKAGE, "testpkg");

			topA.SetChildAttributes(new List<Attribute>()
			{
				new StringInlineAttribute(AttributeId.PACKAGE_NAME, "testpkg"),
				new StringInlineAttribute(AttributeId.PACKAGE_VENDOR, "Test Vendor"),
				new StringInlineAttribute(AttributeId.PACKAGE_SUMMARY, "This is a test package summary"),
				new StringInlineAttribute(AttributeId.PACKAGE_DESCRIPTION, "This is a test package description"),
				new StringInlineAttribute(AttributeId.PACKAGE_URL, "http://www.haiku-os.org"),
				new IntAttribute(AttributeId.PACKAGE_ARCHITECTURE, new BigInteger(1)), // X86
                majorVersionA,
				new StringInlineAttribute(AttributeId.PACKAGE_COPYRIGHT, "Some copyright A"),
				new StringInlineAttribute(AttributeId.PACKAGE_COPYRIGHT, "Some copyright B"),
				new StringInlineAttribute(AttributeId.PACKAGE_LICENSE, "Some license A"),
				new StringInlineAttribute(AttributeId.PACKAGE_LICENSE, "Some license B")
			});

			return topA;
		}

        [Test]
		public void TestCreatePackage()
		{

			Attribute attribute = CreateTestPackageAttributes();
			PkgFactory pkgFactory = new PkgFactory();

			// it is ok that this is empty because the factory should not need to reference the heap again; its all inline
			// and we are not testing the heap here.
			AttributeContext attributeContext = new AttributeContext();

			Pkg pkg = pkgFactory.CreatePackage(
					attributeContext,
					attribute);

			// now do some checks.

			Assert.That(pkg.Architecture == PkgArchitecture.X86);
			Assert.That(pkg.Name == "testpkg");
			Assert.That(pkg.Vendor == "Test Vendor");
			Assert.That(pkg.Summary == "This is a test package summary");
			Assert.That(pkg.Description == "This is a test package description");
			Assert.That(pkg.HomePageUrl.Url == "http://www.haiku-os.org");
			Assert.That(pkg.HomePageUrl.UrlType == PkgUrlType.HOMEPAGE);

			Assert.That(pkg.Version.Major == "6");
			Assert.That(pkg.Version.Minor == "32");
			Assert.That(pkg.Version.Micro == "9");
			Assert.That(pkg.Version.PreRelease == "beta");
			Assert.That(pkg.Version.Revision == 8);

			Assert.That(pkg.Copyrights.Any(x => x == "Some copyright A"));
			Assert.That(pkg.Licenses.Any(x => x == "Some license A"));
			Assert.That(pkg.Licenses.Any(x => x == "Some license B"));
		}
	}
}
