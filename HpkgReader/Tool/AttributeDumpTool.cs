/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using HpkgReader.Compat.CmdLine;
using HpkgReader.Model;
using HpkgReader.Output;
using System;
using System.IO;

namespace HpkgReader.Tool
{
    /// <summary>
    /// Given an HPKR/HPKG file, this small program will dump all of the attributes
    /// of the HPKR/HPKG file.This is handy for diagnostic purposes.
    /// </summary>
    public class AttributeDumpTool : Runnable
    {

        private static readonly Logger LOGGER = LoggerFactory.GetLogger(typeof(AttributeDumpTool));

        [Option(Name = "-f", Required = true, Usage = "the HPKR/HPKG file is required")]
        private FileInfo hpkFile;

        private AttributeDumpTool()
        {
        }

        public static void Main(string[] args)
        {
            AttributeDumpTool main = new AttributeDumpTool();
            CmdLineParser parser = new CmdLineParser(main);

            try
            {
                parser.ParseArgument(args);
                main.Run();
            }
            catch (CmdLineException cle)
            {
                throw new InvalidOperationException("unable to parse arguments", cle);
            }
            catch (Exception th)
            {
                LOGGER.Error("failure in attribute dump tool", th);
            }
        }

        public void Run()
        {
            switch (GetType())
            {
                case FileType.HPKG:
                    RunHpkg();
                    break;
                case FileType.HPKR:
                    RunHpkr();
                    break;
                default:
                    throw new InvalidOperationException("unknown file format");
            }
        }

        private void RunHpkr()
        {
            using (HpkrFileExtractor hpkrFileExtractor = new HpkrFileExtractor(hpkFile))
            using (StreamWriter streamWriter = new StreamWriter(Console.OpenStandardOutput()))
            using (AttributeWriter attributeWriter = new AttributeWriter(streamWriter))
            {
                attributeWriter.Write(hpkrFileExtractor.GetPackageAttributesIterator());
                attributeWriter.Flush();
            }
        }

        private void RunHpkg()
        {
            using (HpkgFileExtractor hpkgFileExtractor = new HpkgFileExtractor(hpkFile))
            using (StreamWriter streamWriter = new StreamWriter(Console.OpenStandardOutput()))
            using (AttributeWriter attributeWriter = new AttributeWriter(streamWriter))
            {
                WriteHeader(streamWriter, "package attributes");
                attributeWriter.Write(hpkgFileExtractor.GetPackageAttributesIterator());
                WriteHeader(streamWriter, "toc");
                attributeWriter.Write(hpkgFileExtractor.GetTocIterator());
                attributeWriter.Flush();
            }
        }

        private void WriteHeader(TextWriter writer, string headline)
        {
            writer.Write(headline);
            writer.Write(":\n");
            writer.Write("-------------------\n");
            writer.Flush();
        }

        private FileType GetType()
        {
            using (FileStream randomAccessFile = RandomAccessFileCompat.Construct(hpkFile, "r"))
            {
                FileHelper fileHelper = new FileHelper();
                return fileHelper.GetType(randomAccessFile);
            }
        }

    }

}
