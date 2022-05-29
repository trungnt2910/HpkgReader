/*
 * Copyright 2021, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using HpkgReader.Heap;
using HpkgReader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using Attribute = HpkgReader.Model.Attribute;

namespace HpkgReader
{
    /// <summary>
    /// <para>This object represents an object that can extract an Hpkg (Haiku Pkg) file.  If you are wanting to
    /// read HPKG files then you should instantiate an instance of this class and then make method calls to it in order to
    /// read values such as the attributes of the file.</para>
    /// </summary>
    public class HpkgFileExtractor : Closable
    {

        private readonly FileInfo file;

        private readonly HpkgHeader header;

        private readonly HpkHeapReader heapReader;

        private readonly HpkStringTable tocStringTable;

        private readonly HpkStringTable packageAttributesStringTable;

        public HpkgFileExtractor(FileInfo file)
            : base()
        {
            Preconditions.CheckNotNull(file);
            // C# file.Exists implies IsFile.
            Preconditions.CheckState(/* file.IsFile && */ file.Exists, "the file does not exist or is not a file");

            this.file = file;
            this.header = ReadHeader();

            try
            {
                heapReader = new HpkHeapReader(
                        file,
                        header.HeapCompression,
                        header.HeaderSize,
                        header.HeapChunkSize, // uncompressed size
                        header.HeapSizeCompressed, // including the compressed chunk lengths.
                        header.HeapSizeUncompressed // excludes the compressed chunk lengths.
                );

                tocStringTable = new HpkStringTable(
                        heapReader,
                        header.HeapSizeUncompressed
                                - (header.PackageAttributesLength + header.TocLength),
                        header.TocStringsLength,
                        header.TocStringsCount);

                packageAttributesStringTable = new HpkStringTable(
                        heapReader,
                        header.HeapSizeUncompressed - header.PackageAttributesLength,
                        header.PackageAttributesStringsLength,
                        header.PackageAttributesStringsCount);
            }
            catch (Exception e)
            {
                Close();
                throw new HpkException("unable to setup the hpkg file extractor", e);
            }
        }

        public override void Close()
        {
            if (null != heapReader)
            {
                heapReader.Close();
            }
        }

        public AttributeContext GetPackageAttributeContext()
        {
            AttributeContext context = new AttributeContext();
            context.HeapReader = heapReader;
            context.StringTable = packageAttributesStringTable;
            return context;
        }

        public AttributeIterator GetPackageAttributesIterator()
        {
            long offset = (header.HeapSizeUncompressed - header.PackageAttributesLength)
                    + header.PackageAttributesStringsLength;
            return new AttributeIterator(GetPackageAttributeContext(), offset);
        }

        public AttributeContext GetTocContext()
        {
            AttributeContext context = new AttributeContext();
            context.HeapReader = heapReader;
            context.StringTable = tocStringTable;
            return context;
        }

        public AttributeIterator GetTocIterator()
        {
            long tocOffset = (header.HeapSizeUncompressed
                    - (header.PackageAttributesLength + header.TocLength));
            long tocAttributeOffset = tocOffset + header.TocStringsLength;
            return new AttributeIterator(GetTocContext(), tocAttributeOffset);
        }

        public List<Attribute> GetToc()
        {
            List<Attribute> assembly = new List<Attribute>();
            AttributeIterator attributeIterator = GetTocIterator();
            while (attributeIterator.HasNext())
            {
                assembly.Add(attributeIterator.Next());
            }
            return assembly;
        }

        private HpkgHeader ReadHeader()
        {
            Preconditions.CheckNotNull(file);
            FileHelper fileHelper = new FileHelper();

            using (FileStream randomAccessFile = RandomAccessFileCompat.Construct(file, "r"))
            {
                if (fileHelper.GetType(randomAccessFile) != FileType.HPKG)
                {
                    throw new HpkException("magic incorrect at the start of the hpkg file");
                }

                HpkgHeader result = new HpkgHeader();

                result.HeaderSize = fileHelper.ReadUnsignedShortToInt(randomAccessFile);
                result.Version = fileHelper.ReadUnsignedShortToInt(randomAccessFile);
                result.TotalSize = fileHelper.ReadUnsignedLongToLong(randomAccessFile);
                result.MinorVersion = fileHelper.ReadUnsignedShortToInt(randomAccessFile);

                result.HeapCompression = HeapCompressionCompat.GetByNumericValue(fileHelper.ReadUnsignedShortToInt(randomAccessFile));
                result.HeapChunkSize = fileHelper.ReadUnsignedIntToLong(randomAccessFile);
                result.HeapSizeCompressed = fileHelper.ReadUnsignedLongToLong(randomAccessFile);
                result.HeapSizeUncompressed = fileHelper.ReadUnsignedLongToLong(randomAccessFile);

                result.PackageAttributesLength = fileHelper.ReadUnsignedIntToLong(randomAccessFile);
                result.PackageAttributesStringsLength = fileHelper.ReadUnsignedIntToLong(randomAccessFile);
                result.PackageAttributesStringsCount = fileHelper.ReadUnsignedIntToLong(randomAccessFile);
                randomAccessFile.SkipBytes(4); // reserved uint32

                result.TocLength = fileHelper.ReadUnsignedLongToLong(randomAccessFile);
                result.TocStringsLength = fileHelper.ReadUnsignedLongToLong(randomAccessFile);
                result.TocStringsCount = fileHelper.ReadUnsignedLongToLong(randomAccessFile);

                return result;
            }
        }
    }
}
