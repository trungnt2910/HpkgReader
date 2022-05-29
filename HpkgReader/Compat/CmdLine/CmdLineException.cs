using System;

namespace HpkgReader.Compat.CmdLine
{
	internal class CmdLineException : Exception
	{
		public CmdLineException(string message)
			: base(message)
		{
		}

		public CmdLineException(string message, Exception cause)
			: base(message, cause)
		{
		}
	}
}
