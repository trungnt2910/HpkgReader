/*
 * Copyright 2018-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using HpkgReader.Model;
using NUnit.Framework;
using System.IO;
using System.Numerics;

namespace HpkgReader
{
    /// <summary>
    /// <para>This is a simplistic test that is just going to stream out some attributes from a known HPKR file and will
    /// then look for certain packages and make sure that an artificially created attribute set matches.  It's basically
    /// a smoke test.</para>
    /// </summary>
    public class HpkrFileExtractorAttributeTest : AbstractHpkTest
	{

		private const string RESOURCE_TEST = "repo.hpkr";

		[Test]
		public void TestReadFile()
		{

			FileInfo hpkrFile = PrepareTestFile(RESOURCE_TEST);

			using (HpkrFileExtractor hpkrFileExtractor = new HpkrFileExtractor(hpkrFile))
			{

				AttributeIterator attributeIterator = hpkrFileExtractor.GetPackageAttributesIterator();
				AttributeContext attributeContext = hpkrFileExtractor.GetAttributeContext();
				Attribute ncursesSourceAttributeOptional = TryFindAttributesForPackage(attributeIterator, "ncurses_source");

				Assert.That(ncursesSourceAttributeOptional != null);
				Attribute ncursesSourceAttribute = ncursesSourceAttributeOptional;

				// now the analysis phase.

				Assert.That(ncursesSourceAttribute != null);
				Assert.That(ncursesSourceAttribute.GetChildAttribute(AttributeId.PACKAGE_NAME).GetValue<string>(attributeContext) == "ncurses_source");
				Assert.That(ncursesSourceAttribute.GetChildAttribute(AttributeId.PACKAGE_ARCHITECTURE).GetValue<BigInteger>(attributeContext) == BigIntegerCompat.Construct("3"));
				Assert.That(ncursesSourceAttribute.GetChildAttribute(AttributeId.PACKAGE_URL).GetValue<string>(attributeContext) == "http://www.gnu.org/software/ncurses/ncurses.html");
				Assert.That(ncursesSourceAttribute.GetChildAttribute(AttributeId.PACKAGE_SOURCE_URL).GetValue<string>(attributeContext) == "Download <http://ftp.gnu.org/pub/gnu/ncurses/ncurses-5.9.tar.gz>");
				Assert.That(ncursesSourceAttribute.GetChildAttribute(AttributeId.PACKAGE_CHECKSUM).GetValue<string>(attributeContext) == "6a25c52890e7d335247bd96965b5cac2f04dafc1de8d12ad73346ed79f3f4215");

				// check the version which is a sub-tree of attributes.

				Attribute majorVersionAttribute = ncursesSourceAttribute.GetChildAttribute(AttributeId.PACKAGE_VERSION_MAJOR);
				Assert.That(majorVersionAttribute.GetValue<string>(attributeContext) == "5");
				Assert.That(majorVersionAttribute.GetChildAttribute(AttributeId.PACKAGE_VERSION_MINOR).GetValue<string>(attributeContext) == "9");
				Assert.That(majorVersionAttribute.GetChildAttribute(AttributeId.PACKAGE_VERSION_REVISION).GetValue<BigInteger>(attributeContext) == BigIntegerCompat.Construct("10"));
			}

		}

		private Attribute TryFindAttributesForPackage(AttributeIterator attributeIterator, string packageName)
		{
			while (attributeIterator.HasNext())
			{
				Attribute attribute = attributeIterator.Next();

				if (AttributeId.PACKAGE == attribute.AttributeId)
				{
					string attributePackageName = attribute.GetValue(attributeIterator.Context).ToString();

					if (attributePackageName.Equals(packageName))
					{
						return attribute;
					}
				}
			}
			return null;
		}
	}
}
