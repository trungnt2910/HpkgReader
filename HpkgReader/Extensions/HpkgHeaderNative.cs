using System.IO;
using System.Runtime.InteropServices;

namespace HpkgReader.Extensions
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct HpkgHeaderNative
    {
        // Do this to avoid any Endian nonsense.
        public byte magic0;
        public byte magic1;
        public byte magic2;
        public byte magic3;

        public ushort header_size;
        public ushort version;
        public ulong total_size;
        public ushort minor_version;

        public ushort heap_compression;
        public uint heap_chunk_size;
        public ulong heap_size_compressed;
        public ulong heap_size_uncompressed;

        public uint attributes_length;
        public uint attributes_strings_length;
        public uint attributes_strings_count;
        public uint reserved1;

        public ulong toc_length;
        public ulong toc_strings_length;
        public ulong toc_strings_count;

        public byte[] ToByteArray()
        {
            var ms = new MemoryStream();
            ms.WriteByte(magic0);
            ms.WriteByte(magic1);
            ms.WriteByte(magic2);
            ms.WriteByte(magic3);

            ms.WriteBigEndian(header_size);
            ms.WriteBigEndian(version);
            ms.WriteBigEndian(total_size);
            ms.WriteBigEndian(minor_version);

            ms.WriteBigEndian(heap_compression);
            ms.WriteBigEndian(heap_chunk_size);
            ms.WriteBigEndian(heap_size_compressed);
            ms.WriteBigEndian(heap_size_uncompressed);

            ms.WriteBigEndian(attributes_length);
            ms.WriteBigEndian(attributes_strings_count);
            ms.WriteBigEndian(attributes_strings_length);
            ms.WriteBigEndian(reserved1);

            ms.WriteBigEndian(toc_length);
            ms.WriteBigEndian(toc_strings_count);
            ms.WriteBigEndian(toc_strings_length);

            return ms.ToArray();
        }
    };
}