using System.IO.Compression;
using InternalDeflater = ICSharpCode.SharpZipLib.Zip.Compression.Deflater;

namespace HpkgReader.Compat
{
    internal class Deflater
    {
        private readonly InternalDeflater _deflater = new InternalDeflater();

        /// <summary>
        /// <para>Compresses the input data and fills specified buffer with compressed data. Returns actual number of bytes of compressed data. A return value of 0 indicates that needsInput should be called in order to determine if more input data is required.</para>
        /// <para>This method uses NO_FLUSH as its compression flush mode. An invocation of this method of the form deflater.deflate(b) yields the same result as the invocation of deflater.deflate(b, 0, b.length, Deflater.NO_FLUSH).</para>
        /// </summary>
        /// <param name="b">the buffer for the compressed data</param>
        /// <returns>the actual number of bytes of compressed data written to the output buffer</returns>
        public int Deflate(byte[] b)
        {
            return _deflater.Deflate(b);
        }

        /// <summary>
        /// Sets input data for compression. This should be called whenever needsInput() returns true indicating that more input data is required.
        /// </summary>
        /// <param name="b">the input data bytes</param>
        public void SetInput(byte[] b)
        {
            _deflater.SetInput(b, 0, b.Length);
        }

        /// <summary>
        /// When called, indicates that compression should end with the current contents of the input buffer.
        /// </summary>
        public void Finish()
        {
            _deflater.Finish();
        }

        /// <summary>
        /// Returns true if the end of the compressed data output stream has been reached.
        /// </summary>
        /// <returns>true if the end of the compressed data output stream has been reached</returns>
        public bool Finished()
        {
            return _deflater.IsFinished;
        }

        /// <summary>
        /// Closes the compressor and discards any unprocessed input. This method should be called when the compressor is no longer being used, but will also be called automatically by the finalize() method. 
        /// Once this method is called, the behavior of the Deflater object is undefined.
        /// </summary>
        public void End()
        {
            _deflater.Reset();
        }
    }
}
