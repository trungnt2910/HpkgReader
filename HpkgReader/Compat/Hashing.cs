using System.Security.Cryptography;

namespace HpkgReader.Compat
{
    internal class Hashing
    {
        public static HashAlgorithm MD5()
        {
            return System.Security.Cryptography.MD5.Create();
        }
    }
}
