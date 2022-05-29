using System;
using System.IO;

namespace HpkgReader.Compat
{
    internal static class RandomAccessFileCompat
    {
        /// <summary>
        /// <para>Creates a random access file stream to read from, and optionally to write to, the file specified by the File argument. A new FileDescriptor object is created to represent this file connection.</para>
        /// <para>The <paramref name="mode"/> argument specifies the access mode in which the file is to be opened.</para>
        /// </summary>
        /// <param name="file">the file object</param>
        /// <param name="mode">the access mode, as described above</param>
        /// <returns></returns>
        public static FileStream Construct(FileInfo file, string mode)
        {
            switch (mode)
            {
                case "r":
                    return file.OpenRead();
                case "rw":
                    return file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
                case "rws":
                case "rwd":
                    // To disable the FileStream buffering, just pass 1 (works for every .NET) or 0 (works for .NET 6 preview 6+) as bufferSize.
                    return new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, false);
                default:
                    throw new ArgumentException("the mode argument is not equal to one of \"r\", \"rw\", \"rws\", or \"rwd\"");
            }
        }

        /// <summary>
        /// Sets the file-pointer offset, measured from the beginning of this file, at which the next read or write occurs.
        /// The offset may be set beyond the end of the file. Setting the offset beyond the end of the file does not change 
        /// the file length. The file length will change only by writing after the offset has been set beyond the end of the file.
        /// </summary>
        /// <param name="fileStream">the RandomAccessFile</param>
        /// <param name="pos">the offset position, measured in bytes from the beginning of the file, at which to set the file pointer.</param>
        public static void Seek(this FileStream fileStream, long pos)
        {
            fileStream.Seek(pos, SeekOrigin.Begin);
        }

        /// <summary>
        /// <para>Attempts to skip over <paramref name="n"/> bytes of input discarding the skipped bytes.</para>
        /// <para>This method may skip over some smaller number of bytes, possibly zero. This may result 
        /// from any of a number of conditions; reaching end of file before <paramref name="n"/> bytes 
        /// have been skipped is only one possibility. This method never throws an EOFException. The 
        /// actual number of bytes skipped is returned. If <paramref name="n"/> is negative, no bytes 
        /// are skipped.</para>
        /// </summary>
        /// <param name="n">the number of bytes to be skipped.</param>
        /// <returns>the actual number of bytes skipped.</returns>
        public static int SkipBytes(this FileStream fileStream, int n)
        {
            var arr = new byte[n];
            return fileStream.Read(arr, 0, n);
        }
    }
}
