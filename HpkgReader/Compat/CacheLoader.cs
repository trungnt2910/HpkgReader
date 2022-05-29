using System;

namespace HpkgReader.Compat
{
    internal class CacheLoader<K, V>
    {
        public Func<K, V> Load { get; set; }
    }
}
