using System;
using System.Text;

namespace HpkgReader.Compat
{
    internal static class StringCompat
    {
        /// <summary>
        /// <para>Constructs a new String by decoding the specified subarray of bytes using the specified charset. The length of the new String is a function of the charset, and hence may not be equal to the length of the subarray.</para>
        /// <para>This method always replaces malformed-input and unmappable-character sequences with this charset's default replacement string. The CharsetDecoder class should be used when more control over the decoding process is required.</para>
        /// </summary>
        /// <param name="bytes">The bytes to be decoded into characters</param>
        /// <param name="offset">The index of the first byte to decode</param>
        /// <param name="length">The number of bytes to decode</param>
        /// <param name="charset">The charset to be used to decode the <paramref name="bytes"/></param>
        /// <returns></returns>
        public static string Construct(byte[] bytes, int offset, int length, Encoding charset)
        {
            return charset.GetString(bytes, offset, length);
        }

        public static string Construct(byte[] bytes, string charset)
        {
            switch (charset)
            {
                case "UTF-8":
                    return Encoding.UTF8.GetString(bytes);
                default:
                    // There are other charsets we don't care about yet.
                    throw new NotImplementedException();
            }
        }

        public static string EmptyToNull(string str)
        {
            return str == string.Empty ? null : str;
        }

        public static byte[] GetBytes(this string str, string encoding)
        {
            switch (encoding)
            {
                case "UTF-8":
                    return Encoding.UTF8.GetBytes(str);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
