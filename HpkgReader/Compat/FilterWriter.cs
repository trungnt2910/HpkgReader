using System;
using System.IO;
using System.Text;

namespace HpkgReader.Compat
{
    public class FilterWriter : TextWriter
    {
        private TextWriter _writer;

        public override Encoding Encoding => _writer.Encoding;

        /// <summary>
        /// The underlying character-output stream.
        /// </summary>
        protected TextWriter Out => _writer;

        /// <summary>
        /// Create a new filtered writer.
        /// </summary>
        /// <param name="out">a <see cref="TextWriter"/> object to provide the underlying stream.</param>
        /// <exception cref="NullReferenceException">out is null</exception>
        protected FilterWriter(TextWriter @out)
        {
            if (@out == null)
            {
                throw new NullReferenceException($"{nameof(@out)} is null");
            }

            _writer = @out;
        }
    }
}
