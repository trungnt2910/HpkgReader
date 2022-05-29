using System.IO;
using System.Security.Cryptography;

namespace HpkgReader.Compat
{
    public abstract class ByteSource
    {
        protected ByteSource()
        {
        }

        /// <summary>
        /// Opens a new <see cref="Stream"/> for reading from this source.
        /// </summary>
        /// <returns></returns>
        public abstract Stream OpenStream();

        /// <summary>
        /// <para>Returns the size of this source in bytes, if the size can be easily determined without actually opening the data stream.</para>
        /// <para>The default implementation returns Optional.absent(). Some sources, such as a file, may return a non-absent value. 
        /// Note that in such cases, it is possible that this method will return a different number of bytes than would be returned 
        /// by reading all of the bytes (for example, some special files may return a size of 0 despite actually having content when read).</para>
        /// <para>Additionally, for mutable sources such as files, a subsequent read may return a different number of bytes if the contents are changed.</para>
        /// </summary>
        /// <returns></returns>
        public virtual long? SizeIfKnown()
        {
            return null;
        }

        public virtual long Size()
        {
            // The default implementation calls sizeIfKnown() and returns the value if present.
            // If absent, it will fall back to a heavyweight operation that will open a stream, read
            // (or skip, if possible) to the end of the stream and return the total number of bytes
            // that were read.
            // TODO: Use the real implementation.
            return SizeIfKnown() ?? 0;
        }

        /// <summary>
        /// Returns a view of the given byte array as a <see cref="ByteSource"/>.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static ByteSource Wrap(byte[] b)
        {
            var source = new ByteArrayByteSource(b);
            return source;
        }

        /// <summary>
        /// Reads the full contents of this byte source as a byte array.
        /// </summary>
        /// <returns></returns>
        public virtual byte[] Read()
        {
            var stream = OpenStream();
            var memory = new MemoryStream();
            stream.CopyTo(memory);
            stream.Dispose();
            return memory.ToArray();
        }

        public HashCode Hash(HashAlgorithm hashFunction)
        {
            return new HashCode(hashFunction.ComputeHash(Read()));
        }
    }
}
