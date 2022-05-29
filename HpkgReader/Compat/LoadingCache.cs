using Microsoft.Extensions.Caching.Memory;

namespace HpkgReader.Compat
{
    internal class LoadingCache<K, V>
    {
        internal CacheLoader<K, V> Loader { get; set; }

        private MemoryCache _cache;

        internal LoadingCache(long? maximumSize)
        {
            _cache = new MemoryCache(new MemoryCacheOptions()
            {
                SizeLimit = maximumSize
            });
        }

        /// <summary>
        /// <para>Returns the value associated with key in this cache, first loading that value if necessary.
        /// No observable state associated with this cache is modified until loading completes.
        /// Unlike get(K), this method does not throw a checked exception, and thus should only be used
        /// in situations where checked exceptions are not thrown by the cache loader.</para>
        /// <para>If another call to get(K) or getUnchecked(K) is currently loading the value for key, 
        /// simply waits for that thread to finish and returns its loaded value. Note that multiple 
        /// threads can concurrently load values for distinct keys.</para>
        /// <para>Caches loaded by a <see cref="CacheLoader{K, V}"/> will call <see cref="CacheLoader{K, V}.Load"/> 
        /// to load new values into the cache. Newly loaded values are added to the cache using 
        /// Cache.asMap().putIfAbsent after loading has completed; if another value was associated with 
        /// key while the new value was loading then a removal notification will be sent for the new value.</para>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public V GetUnchecked(K key)
        {
            lock (_cache)
            {
                return _cache.GetOrCreate(key, CacheEntryFactory);
            }
        }

        private V CacheEntryFactory(ICacheEntry entry)
        {
            entry.Size = 1;
            return Loader.Load((K)entry.Key);
        }
    }
}
