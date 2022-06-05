using HpkgReader.Heap;
using HpkgReader.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace HpkgReader.Extensions
{
    /// <summary>
    /// Writes a <see cref="BetterPkg"/> to a file.
    /// </summary>
    public static class HpkgWriter
    {
        // 64KB
        private const int CHUNK_SIZE = 64 * 1024;
        // Currently writing for version 2.1.
        // See: https://github.com/haiku/haiku/blob/master/docs/develop/packages/FileFormat.rst
        private const ushort WRITER_VERSION = 2;
        private const ushort WRITER_MINOR_VERSION = 1;

        /// <summary>
        /// Writes the contents of a <see cref="BetterPkg"/> to a file.
        /// </summary>
        /// <param name="package">The <see cref="BetterPkg"/>.</param>
        /// <param name="outputFile">Path to a output file.</param>
        /// <param name="tempDir">The temporary directory for heap data.</param>
        public static void Write(BetterPkg package, string outputFile, string tempDir = null)
        {
            var header = new HpkgHeaderNative();

            // Fill default values
            header.magic0 = (byte)'h';
            header.magic1 = (byte)'p';
            header.magic2 = (byte)'k';
            header.magic3 = (byte)'g';
            header.header_size = (ushort)Marshal.SizeOf<HpkgHeaderNative>();
            header.version = WRITER_VERSION;
            header.minor_version = WRITER_MINOR_VERSION;
            header.heap_compression = (ushort)HeapCompression.ZLIB;
            header.heap_chunk_size = CHUNK_SIZE;

            // This is a naive implementation that writes every value (strings, bytes,...) inline.
            var heap = new HpkgHeapBuilder(tempDir, HeapCompression.ZLIB, CHUNK_SIZE);
            // Build the TOC first.
            header.toc_length = 
                (ulong)heap.WriteAttributes(package.DirectoryEntries.Select(de => new BetterAttribute(de)));
            header.toc_strings_length = 0;
            header.toc_strings_count = 0;

            header.attributes_length = 0;
            header.attributes_strings_count = 0;
            header.attributes_strings_length = 0;

            var packageAttributes = new List<BetterAttribute>();

            void TryAdd(AttributeId id, object value)
            {
                if (value == null)
                {
                    return;
                }
                packageAttributes.Add(new BetterAttribute(id, value).GuessTypeAndEncoding());
            }

            TryAdd(AttributeId.PACKAGE_NAME, package.Name);
            TryAdd(AttributeId.PACKAGE_SUMMARY, package.Summary);
            TryAdd(AttributeId.PACKAGE_DESCRIPTION, package.Description);
            TryAdd(AttributeId.PACKAGE_VENDOR, package.Vendor);
            TryAdd(AttributeId.PACKAGE_PACKAGER, package.Packager);
            TryAdd(AttributeId.PACKAGE_BASE_PACKAGE, package.BasePackage);
            TryAdd(AttributeId.PACKAGE_FLAGS, (uint?)package.Flags);
            TryAdd(AttributeId.PACKAGE_ARCHITECTURE, (uint?)package.Architecture);
            if (package.BetterVersion != null)
            {
                packageAttributes.Add(new BetterAttribute(package.BetterVersion));
            }
            foreach (var copyright in package.Copyrights)
            {
                TryAdd(AttributeId.PACKAGE_COPYRIGHT, copyright);
            }
            foreach (var licenses in package.Licenses)
            {
                TryAdd(AttributeId.PACKAGE_LICENSE, licenses);
            }
            TryAdd(AttributeId.PACKAGE_URL, package.HomePageUrl.ToString());
            TryAdd(AttributeId.PACKAGE_SOURCE_URL, package.SourceUrl.ToString());

            header.attributes_length += (uint)heap.WriteAttributes(packageAttributes);

            heap.Complete();

            header.heap_size_compressed = (ulong)heap.CompressedSize;
            header.heap_size_uncompressed = (ulong)heap.UncompressedSize;
            header.total_size = header.heap_size_compressed + header.header_size;

            using (var stream = File.Create(outputFile))
            {
                var headerBytes = header.ToByteArray();
                stream.Write(headerBytes, 0, headerBytes.Length);
                heap.WriteToStream(stream);
            }

            heap.Dispose();
        }
    }
}
