/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using HpkgReader.Heap;
using HpkgReader.Model;
using System;
using System.IO;

namespace HpkgReader
{
    /// <summary>
    ///  <para>This object represents an object that can extract an Hpkr (Haiku Pkg Repository) file.  If you are wanting to
    ///  read HPKR files then you should instantiate an instance of this class and then make method calls to it in order to
    ///  read values such as the attributes of the HPKR file.</para>
    /// </summary>
    public class HpkrFileExtractor : Closable
    {

        private readonly FileInfo file;

        private readonly HpkrHeader header;

        private readonly HpkHeapReader heapReader;

        private readonly HpkStringTable attributesStringTable;

        public HpkrFileExtractor(FileInfo file)
            : base()
        {
            Preconditions.CheckNotNull(file);
            Preconditions.CheckState(file.IsFile() && file.Exists, "the file does not exist or is not a file");

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

                attributesStringTable = new HpkStringTable(
                        heapReader,
                        header.InfoLength,
                        header.PackagesStringsLength,
                        header.PackagesStringsCount);

            }
            catch (Exception e)
            {
                Close();
                throw new HpkException("unable to setup the hpkr file extractor", e);
            }
        }

        public override void Close()
        {
            if (null != heapReader)
            {
                heapReader.Close();
            }
        }

        public AttributeContext GetAttributeContext()
        {
            AttributeContext context = new AttributeContext();
            context.HeapReader = heapReader;
            context.StringTable = attributesStringTable;
            return context;
        }

        public AttributeIterator GetPackageAttributesIterator()
        {
            long offset = header.InfoLength + header.PackagesStringsLength;
            return new AttributeIterator(GetAttributeContext(), offset);
        }

        private HpkrHeader ReadHeader()
        {
            Preconditions.CheckNotNull(file);
            FileHelper fileHelper = new FileHelper();

            using (FileStream randomAccessFile = RandomAccessFileCompat.Construct(file, "r"))
            {

                if (fileHelper.GetType(randomAccessFile) != FileType.HPKR)
                {
                    throw new HpkException("magic incorrect at the start of the hpkr file");
                }

                HpkrHeader result = new HpkrHeader();

                result.HeaderSize = fileHelper.ReadUnsignedShortToInt(randomAccessFile);
                result.Version = fileHelper.ReadUnsignedShortToInt(randomAccessFile);
                result.TotalSize = fileHelper.ReadUnsignedLongToLong(randomAccessFile);
                result.MinorVersion = fileHelper.ReadUnsignedShortToInt(randomAccessFile);

                result.HeapCompression = HeapCompressionCompat.GetByNumericValue(fileHelper.ReadUnsignedShortToInt(randomAccessFile));
                result.HeapChunkSize = fileHelper.ReadUnsignedIntToLong(randomAccessFile);
                result.HeapSizeCompressed = fileHelper.ReadUnsignedLongToLong(randomAccessFile);
                result.HeapSizeUncompressed = fileHelper.ReadUnsignedLongToLong(randomAccessFile);

                // repository info
                result.InfoLength = fileHelper.ReadUnsignedIntToLong(randomAccessFile);
                randomAccessFile.SkipBytes(4); // reserved

                // package attributes section
                result.PackagesLength = fileHelper.ReadUnsignedLongToLong(randomAccessFile);
                result.PackagesStringsLength = fileHelper.ReadUnsignedLongToLong(randomAccessFile);
                result.PackagesStringsCount = fileHelper.ReadUnsignedLongToLong(randomAccessFile);

                return result;
            }
        }
    }
}
