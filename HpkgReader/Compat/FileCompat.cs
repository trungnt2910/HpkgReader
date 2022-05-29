using System.IO;

namespace HpkgReader.Compat
{
    public static class FileCompat
    {
        public static bool IsFile(this FileInfo info)
        {
            return info.Exists;
        }
    }
}
