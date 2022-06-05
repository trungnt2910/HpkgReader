using HpkgReader.Extensions;
using HpkgReader.Model;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HpkgReader.Tests.Extensions
{
    public class HpkgWriterTest: AbstractHpkTest
    {
        private const string RESOURCE_TEST = "tipster-1.1.1-1-x86_64.hpkg";

        [Test]
        public void TestWriter_Attributes()
        {
            var info = PrepareTestFile(RESOURCE_TEST);
            var tempFile = Path.GetTempFileName();
            using var betterPkg = new BetterPkg(info.FullName);
            HpkgWriter.Write(betterPkg, tempFile);

            {
                using var packageOld = new HpkgFileExtractor(info);
                using var packageNew = new HpkgFileExtractor(new FileInfo(tempFile));

                var attributes1 = packageOld.GetPackageAttributes();
                var ctx1 = packageOld.GetPackageAttributeContext();
                var attributes2 = packageNew.GetPackageAttributes();
                var ctx2 = packageNew.GetPackageAttributeContext();

                Assert.IsTrue(Compare(attributes1, ctx1, attributes2, ctx2));
            }

            File.Delete(tempFile);
        }

        private bool Compare(
            List<Attribute> attributes1, AttributeContext ctx1,
            List<Attribute> attributes2, AttributeContext ctx2
            )
        {
            if (attributes1.Count != attributes2.Count)
            {
                return false;
            }

            foreach (var attr1 in attributes1)
            {
                bool good = false;
                foreach (var attr2 in attributes2)
                {
                    if (attr1.AttributeId != attr2.AttributeId)
                    {
                        continue;
                    }
                    if (attr1.AttributeType != attr2.AttributeType)
                    {
                        continue;
                    }
                    if (!attr1.GetValue(ctx1).Equals(attr2.GetValue(ctx2)))
                    {
                        continue;
                    }
                    if (attr1.HasChildAttributes() != attr2.HasChildAttributes())
                    {
                        continue;
                    }
                    if (attr1.HasChildAttributes() && 
                        !Compare(attr1.GetChildAttributes(), ctx1, attr2.GetChildAttributes(), ctx2))
                    {
                        continue;
                    }
                    good = true;
                    break;
                }
                if (!good)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
