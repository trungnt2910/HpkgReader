/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat.CmdLine;
using HpkgReader.Output;
using System;
using System.IO;

namespace HpkgReader.Tool
{
    /// <summary>
    /// This small tool will take an HPKR file and parse it first into attributes and then into packages.  The packages
    /// are dumped out to the standard output.  This is useful as means of debugging.
    /// </summary>
    public class PkgDumpTool
    {

        protected readonly static Logger LOGGER = LoggerFactory.GetLogger(typeof(AttributeDumpTool));

        [Option(Name = "-f", Required = true, Usage = "the HPKR file is required")]
        private FileInfo hpkrFile;

        public static void Main(string[] args)
        {
            PkgDumpTool main = new PkgDumpTool();
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
        }

        public void Run()
        {
            new CmdLineParser(this);

            try
            {
                using (HpkrFileExtractor hpkrFileExtractor = new HpkrFileExtractor(hpkrFile))
                using (StreamWriter streamWriter = new StreamWriter(Console.OpenStandardOutput()))
                using (PkgWriter pkgWriter = new PkgWriter(streamWriter))
                {
                    pkgWriter.Write(new PkgIterator(hpkrFileExtractor.GetPackageAttributesIterator()));
                    pkgWriter.Flush();
                }
            }
            catch (Exception th)
            {
                LOGGER.Error("unable to dump packages", th);
            }
        }



    }

}