using System;
using InternalInflater = ICSharpCode.SharpZipLib.Zip.Compression.Inflater;

namespace HpkgReader.Compat
{
    internal class Inflater
    {
        private InternalInflater _inflater = new InternalInflater();
        private byte[] _temp = new byte[1];
        private bool _hasTemp = false;

        /// <summary>
        /// Creates a new decompressor.
        /// </summary>
        public Inflater()
        {
        }

        /// <summary>
        /// Sets input data for decompression. Should be called whenever <see cref="NeedsInput"/> returns true indicating that more input data is required.
        /// </summary>
        /// <param name="b">the input data bytes</param>
        public void SetInput(byte[] b)
        {
            _inflater.SetInput(b, 0, b.Length);
        }

        /// <summary>
        /// Sets input data for decompression. Should be called whenever <see cref="NeedsInput"/> returns true indicating that more input data is required.
        /// </summary>
        /// <param name="b">the input data bytes</param>
        /// <param name="off">the start offset of the input data</param>
        /// <param name="len">the length of the input data</param>
        public void SetInput(byte[] b, int off, int len)
        {
            _inflater.SetInput(b, off, len);
        }

        /// <summary>
        /// Returns true if no data remains in the input buffer. This can be used to determine if <see cref="SetInput(byte[])"/> should be called in order to provide more input.
        /// </summary>
        /// <returns><c>true</c> if no data remains in the input buffer</returns>
        public bool NeedsInput()
        {
            return _inflater.IsNeedingInput;
        }

        /// <summary>
        /// Returns true if a preset dictionary is needed for decompression.
        /// </summary>
        /// <returns><c>true</c> if a preset dictionary is needed for decompression</returns>
        public bool NeedsDictionary()
        {
            return _inflater.IsNeedingDictionary;
        }

        /// <summary>
        /// Uncompresses bytes into specified buffer. Returns actual number of bytes uncompressed. A return value of 0 indicates that <see cref="NeedsInput"/>
        /// or needsDictionary() should be called in order to determine if more input data or a preset dictionary is required. In the latter case, 
        /// getAdler() can be used to get the Adler-32 value of the dictionary required.
        /// </summary>
        /// <param name="b">the buffer for the uncompressed data</param>
        /// <returns>the actual number of uncompressed bytes</returns>
        public int Inflate(byte[] b)
        {
            if (_hasTemp && b.Length > 0)
            {
                b[0] = _temp[0];
                _hasTemp = false;
                return 1 + _inflater.Inflate(b, 1, b.Length - 1);
            }
            if (NeedsInput())
            {
                return 0;
            }
            return _inflater.Inflate(b, 0, b.Length);
        }

        /// <summary>
        /// Returns true if the end of the compressed data stream has been reached.
        /// </summary>
        /// <returns><c>true</c> if the end of the compressed data stream has been reached</returns>
        public bool Finished()
        {
            // This is a workaround for SharpZipLib's problem: IsFinished is only 
            // set when a read is done when the stream cannot produce more data.
            if (_hasTemp)
            {
                return false;
            }
            if (_inflater.Inflate(_temp) == 1)
            {
                _hasTemp = true;
                return false;
            }
            return _inflater.IsFinished;
        }

        /// <summary>
        /// Closes the decompressor and discards any unprocessed input. This method should be called when the decompressor is 
        /// no longer being used, but will also be called automatically by the finalize() method. Once this method is called, 
        /// the behavior of the Inflater object is undefined.
        /// </summary>
        public void End()
        {
            _inflater.Reset();
            _hasTemp = false;
        }
    }
}
