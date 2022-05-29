using HpkgReader.Heap;

namespace HpkgReader
{
	/// <summary>
	/// Converted from Java source by replacing:
	/// <code>public ([a-z]*) get([A-Za-z_]*)\(\)[\s\n]*{[\s\n]*return ([A-Za-z]*);[\s\n]*}[\s\n]*public void set[A-Za-z_]*\([^\)]*\)[\s\n]*{[\s\n]*.*[\s\n]*\}</code>
	/// with <code>public $1 $2\n{\nget => $3;\nset => $3 = value;\n}</code>
	/// </summary>
	public class HpkgHeader
	{
		private long headerSize;
		private int version;
		private long totalSize;
		private int minorVersion;

		// heap
		private HeapCompression heapCompression;
		private long heapChunkSize;
		private long heapSizeCompressed;
		private long heapSizeUncompressed;

		private long packageAttributesLength;
		private long packageAttributesStringsLength;
		private long packageAttributesStringsCount;

		private long tocLength;
		private long tocStringsLength;
		private long tocStringsCount;

		public long HeaderSize
		{
			get => headerSize;
			set => headerSize = value;
		}

		public int Version
		{
			get => version;
			set => version = value;
		}

		public long TotalSize
		{
			get => totalSize;
			set => totalSize = value;
		}

		public int MinorVersion
		{
			get => minorVersion;
			set => minorVersion = value;
		}

		public HeapCompression HeapCompression
		{
			get => heapCompression;
			set => heapCompression = value;
		}

		public long HeapChunkSize
		{
			get => heapChunkSize;
			set => heapChunkSize = value;
		}

		public long HeapSizeCompressed
		{
			get => heapSizeCompressed;
			set => heapSizeCompressed = value;
		}

		public long HeapSizeUncompressed
		{
			get => heapSizeUncompressed;
			set => heapSizeUncompressed = value;
		}

		public long PackageAttributesLength
		{
			get => packageAttributesLength;
			set => packageAttributesLength = value;
		}

		public long PackageAttributesStringsLength
		{
			get => packageAttributesStringsLength;
			set => packageAttributesStringsLength = value;
		}

		public long PackageAttributesStringsCount
		{
			get => packageAttributesStringsCount;
			set => packageAttributesStringsCount = value;
		}

		public long TocLength
		{
			get => tocLength;
			set => tocLength = value;
		}

		public long TocStringsLength
		{
			get => tocStringsLength;
			set => tocStringsLength = value;
		}

		public long TocStringsCount
		{
			get => tocStringsCount;
			set => tocStringsCount = value;
		}
	}
}