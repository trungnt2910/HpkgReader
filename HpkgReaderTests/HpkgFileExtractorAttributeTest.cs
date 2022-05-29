/*
 * Copyright 2021-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using HpkgReader.Model;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Linq;

namespace HpkgReader
{
    public class HpkgFileExtractorAttributeTest : AbstractHpkTest
	{
		private const string RESOURCE_TEST = "tipster-1.1.1-1-x86_64.hpkg";

		private static readonly int[] HVIF_MAGIC = {
				0x6e, 0x63, 0x69, 0x66
		};

		[Test]
		public void TestReadFile()
		{

			FileInfo hpkgFile = PrepareTestFile(RESOURCE_TEST);

			using (HpkgFileExtractor hpkgFileExtractor = new HpkgFileExtractor(hpkgFile))
			{

				AttributeContext tocContext = hpkgFileExtractor.GetTocContext();
				List<Attribute> tocAttributes = ToList(hpkgFileExtractor.GetTocIterator());
				IntAttribute mTimeAttribute = (IntAttribute)tocAttributes[0].GetChildAttributes()[2];
				Assert.That(mTimeAttribute.AttributeId == AttributeId.FILE_MTIME);
				Assert.That(mTimeAttribute.AttributeType == AttributeType.INT);
				Assert.That(mTimeAttribute.GetValue<BigInteger>(tocContext) == 1551679116L);

				AttributeContext packageAttributeContext = hpkgFileExtractor.GetPackageAttributeContext();
				List<Attribute> packageAttributes = ToList(hpkgFileExtractor.GetPackageAttributesIterator());
				Attribute summaryAttribute = packageAttributes[1];
				Assert.That(summaryAttribute.AttributeId == AttributeId.PACKAGE_SUMMARY);
				Assert.That(summaryAttribute.AttributeType == AttributeType.STRING);
				Assert.That(summaryAttribute.GetValue<string>(packageAttributeContext) == "An application to display Haiku usage tips");

				// Pull out the actual binary to check.  The expected data results were obtained
				// from a Haiku host with the package installed.
				Attribute tipsterDirectoryEntry = FindByDirectoryEntries(tocAttributes, tocContext, new List<string>() { "apps", "Tipster" });

				Attribute binaryData = tipsterDirectoryEntry.GetChildAttribute(AttributeId.DATA);
				ByteSource binaryDataByteSource = (ByteSource)binaryData.GetValue(tocContext);
				Assert.That(binaryDataByteSource.Size() == 153840L);
				HashCode hashCode = binaryDataByteSource.Hash(Hashing.MD5());
				Assert.That(hashCode.ToString().ToLowerInvariant() == "13b16cd7d035ddda09a744c49a8ebdf2");

				var iconAttributeOptional = tipsterDirectoryEntry.GetChildAttributes(AttributeId.FILE_ATTRIBUTE)
						.Select(a => (StringAttribute) a)
						.Where(a => a.GetValue<string>(tocContext) == "BEOS:ICON")
						.FirstOrDefault();

				Assert.That(iconAttributeOptional != null);
				Attribute iconAttribute = iconAttributeOptional.Get();

				Attribute iconBinaryData = iconAttribute.GetChildAttribute(AttributeId.DATA);
				ByteSource iconDataByteSource = (ByteSource)iconBinaryData.GetValue(tocContext);
				byte[] iconBytes = iconDataByteSource.Read();
				AssertIsHvif(iconBytes);
			}
		}

		private Attribute FindByDirectoryEntries(
				List<Attribute> attributes,
				AttributeContext context,
				List<string> pathComponents)
		{
			Preconditions.CheckArgument(!pathComponents.IsEmpty());
			var resultOptional = attributes
					.Where(a => a.AttributeId == AttributeId.DIRECTORY_ENTRY)
					.Where(a => a.GetValue<string>(context) == pathComponents[0])
					.FirstOrDefault();

			if (resultOptional.IsPresent())
			{
				if (1 == pathComponents.Size())
				{
					return resultOptional.Get();
				}
				return FindByDirectoryEntries(
						resultOptional.Get().GetChildAttributes(),
						context,
						pathComponents.SubList(1, pathComponents.Size()));
			}

			throw new AssertionException("unable to find the diretory entry [" + pathComponents[0] + "]");
		}

		private List<Attribute> ToList(AttributeIterator attributeIterator)
		{
			List<Attribute> assembly = new List<Attribute>();
			while (attributeIterator.HasNext())
			{
				assembly.Add(attributeIterator.Next());
			}
			return assembly;
		}

		private void AssertIsHvif(byte[] payload)
		{
			Assert.That(payload.Length > HVIF_MAGIC.Length);
			for (int i = 0; i < HVIF_MAGIC.Length; i++)
			{
				if ((0xff & payload[i]) != HVIF_MAGIC[i])
				{
					Assert.Fail("mismatch on the magic in the data payload");
				}
			}
		}

	}

}