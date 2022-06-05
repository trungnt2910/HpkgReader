using HpkgReader.Model;
using System.Collections.Generic;

namespace HpkgReader.Extensions
{
    /// <summary>
    /// Useful extension functions not available in the Java library for <see cref="HpkgFileExtractor"/>.
    /// </summary>
    public static class PackageExtractorExtensions
    {
        public static List<Attribute> GetPackageAttributes(this HpkgFileExtractor extractor)
        {
            List<Attribute> assembly = new List<Attribute>();
            AttributeIterator attributeIterator = extractor.GetPackageAttributesIterator();
            while (attributeIterator.HasNext())
            {
                assembly.Add(attributeIterator.Next());
            }
            return assembly;
        }

        public static Pkg CreatePkg(this HpkgFileExtractor extractor)
        {
            var factory = new PkgFactory();
            var rootAttribute = new StringInlineAttribute(AttributeId.PACKAGE, "rootAttribute");
            rootAttribute.SetChildAttributes(extractor.GetPackageAttributes());
            return factory.CreatePackage(extractor.GetPackageAttributeContext(), rootAttribute);
        }
    }
}
