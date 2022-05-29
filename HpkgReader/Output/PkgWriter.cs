/*
 * Copyright 2018, Andrew Lindesay
 * Distributed under the terms of the MIT License.
 */

using HpkgReader.Compat;
using HpkgReader.Model;
using System.IO;

namespace HpkgReader.Output
{
	/// <summary>
	/// <para>This is a writer in the sense of being able to produce a human-readable output of a package.</para>
	/// </summary>
	public class PkgWriter : FilterWriter
	{
		public PkgWriter(TextWriter writer)
			: base(writer)
		{
		}

		private void Write(Pkg pkg)
		{
			Preconditions.CheckNotNull(pkg);
			Write(pkg.ToString());
		}

		public void Write(PkgIterator pkgIterator)
		{
			Preconditions.CheckNotNull(pkgIterator);

			try
			{
				while (pkgIterator.HasNext())
				{
					Write(pkgIterator.Next());
					Write('\n');
				}
			}
			catch (PkgException pe)
			{
				throw new IOException("unable to write a package owing to an exception obtaining the package", pe);
			}
		}
	}
}
