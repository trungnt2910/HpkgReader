# HpkgReader - Haiku `.hpkg` package reader for .NET

This package contains controller, model and helper objects for reading package files for the
[Haiku]("http://www.haiku-os.org") project.  Pkg files come in two types.  HPKR is a file
format for providing a kind of catalogue of what is in a repository.  HPKG format is a file that describes
a particular package.  At the time of writing, this library only supports HPKR although there is enough
supporting material to easily provide a reader for HPKG.

Note that this library (currently) only supports (signed) 32bit addressing in the HPKR files.

The .NET version has been ported from [HaikuDepotServer's Java source](https://github.com/haiku/haikudepotserver/tree/034e9da9ed56e84b18707473d4376ccb554b9ee4/haikudepotserver-packagefile).