/*
 * Copyright 2018-2022, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using NUnit.Framework;
using System.Reflection;
using System.IO;

namespace HpkgReader.Tests
{
    public abstract class AbstractHpkTest
    {
        DirectoryInfo temporaryFolder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);


        /**
		 * <p>This will copy the supplied test classpath resource into a temporary file to work with during the test.  It
		 * is the responsibility of the caller to clean up the temporary file afterwards.</p>
		 */
        protected FileInfo PrepareTestFile(string resource)
        {
            Preconditions.CheckState(null != temporaryFolder);
            using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"HpkgReader.Tests.Resources.{resource}");
            var ms = new MemoryStream();
            resourceStream.CopyTo(ms);
            byte[] payload = ms.ToArray();
            var fileInfo = new FileInfo(Path.Combine(temporaryFolder.FullName, resource));
            File.WriteAllBytes(fileInfo.FullName, payload);
            return fileInfo;
        }
    }
}