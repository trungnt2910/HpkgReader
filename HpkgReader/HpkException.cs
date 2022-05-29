/*
 * Copyright 2013, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using System;

namespace HpkgReader
{
	/// <summary>
	/// This type of exception is used through the Hpk file processing system to indicate that something has gone wrong
	/// with processing the Hpk data in some way.
	/// </summary>
	public class HpkException : Exception
	{
		public HpkException(string message)
			: base(message)
		{
		}

		public HpkException(string message, Exception cause)
			: base(message, cause)
		{
		}
	}
}
