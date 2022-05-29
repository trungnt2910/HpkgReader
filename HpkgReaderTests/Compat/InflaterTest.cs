using NUnit.Framework;
using HpkgReader.Compat;
using System;
using System.Text;
using System.IO;

namespace HpkgReaderTests.Compat
{
    internal class InflaterTest
    {
        [Test]
        public void TestDefaultDictionary()
        {
            AssertRoundTrip();
        }

        private static void AssertRoundTrip()
        {
            // Construct a nice long input byte sequence.
            string expected = MakeString();
            byte[] expectedBytes = expected.GetBytes("UTF-8");

            // Compress the bytes, using the passed-in dictionary (or no dictionary).
            byte[] deflatedBytes = Deflate(expectedBytes);

            // Get ready to decompress deflatedBytes back to the original bytes ...
            Inflater inflater = new Inflater();
            // We'll only supply the input a little bit at a time, so that zlib has to ask for more.
            const int CHUNK_SIZE = 16;
            int offset = 0;
            inflater.SetInput(deflatedBytes, offset, CHUNK_SIZE);
            offset += CHUNK_SIZE;
            // Do the actual decompression, now the dictionary's set up appropriately...
            // We use a tiny output buffer to ensure that we call inflate multiple times, and
            // a tiny input buffer to ensure that zlib has to ask for more input.
            MemoryStream inflatedBytes = new MemoryStream();
            byte[] buf = new byte[8];
            while (inflatedBytes.Length != expectedBytes.Length)
            {
                if (inflater.NeedsInput())
                {
                    int nextChunkByteCount = Math.Min(CHUNK_SIZE, deflatedBytes.Length - offset);
                    inflater.SetInput(deflatedBytes, offset, nextChunkByteCount);
                    offset += nextChunkByteCount;
                }
                else
                {
                    int inflatedByteCount = inflater.Inflate(buf);
                    if (inflatedByteCount > 0)
                    {
                        inflatedBytes.Write(buf, 0, inflatedByteCount);
                    }
                }
            }
            inflater.End();

            Assert.AreEqual(expected, StringCompat.Construct(inflatedBytes.ToArray(), "UTF-8"));
        }

        private static string MakeString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 1024; ++i)
            {
                // This is arbitrary but convenient in that it gives our string
                // an easily-recognizable beginning and end.
                sb.Append(i + 1024);
            }
            return sb.ToString();
        }

        private static byte[] Deflate(byte[] input)
        {
            Deflater deflater = new Deflater();
            MemoryStream deflatedBytes = new MemoryStream();
            try
            {
                deflater.SetInput(input);
                deflater.Finish();
                byte[] buf = new byte[8];
                while (!deflater.Finished())
                {
                    int byteCount = deflater.Deflate(buf);
                    deflatedBytes.Write(buf, 0, byteCount);
                }
            }
            finally
            {
                deflater.End();
            }
            return deflatedBytes.ToArray();
        }
    }
}
