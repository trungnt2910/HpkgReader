/*
 * Copyright 2013, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using System;

namespace HpkgReader
{
	public class PkgException : Exception
	{

		public PkgException(string message)
			: base(message)
		{
		}

		public PkgException(string message, Exception cause)
			: base(message, cause)
		{
		}
	}
}