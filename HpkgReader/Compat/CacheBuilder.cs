using System;

namespace HpkgReader.Compat
{
    internal class CacheBuilder<K, V>
    {
        private long _maximumSize = -1;

        /// <summary>
        /// Constructs a new CacheBuilder instance with default settings, including strong keys, strong values, and no automatic eviction of any kind.
        /// </summary>
        /// <returns></returns>
        public static CacheBuilder<K, V> NewBuilder()
        {
            return new CacheBuilder<K, V>();
        }

        /// <summary>
        /// <para>Specifies the maximum number of entries the cache may contain.
        /// Note that the cache may evict an entry before this limit is exceeded. 
        /// As the cache size grows close to the maximum, the cache evicts entries 
        /// that are less likely to be used again. For example, the cache may evict 
        /// an entry because it hasn't been used recently or very often.</para>
        /// <para>When size is zero, elements will be evicted immediately after being loaded 
        /// into the cache.</para>
        /// <para>This can be useful in testing, or to disable caching temporarily without 
        /// a code change.</para>
        /// <para>This feature cannot be used in conjunction with maximumWeight.</para>
        /// </summary>
        /// <param name="size">the maximum size of the cache</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">if <paramref name="size"/> is negative</exception>
        /// <exception cref="InvalidOperationException">if a maximum size or weight was already set</exception>
        public CacheBuilder<K, V> MaximumSize(long size)
        {
            if (_maximumSize != -1)
            {
                throw new InvalidOperationException("Maximum size already set");
            }

            if (size < 0)
            {
                throw new ArgumentException($"{nameof(size)} is negative");
            }

            _maximumSize = size;

            return this;
        }

        /// <summary>
        /// <para>Builds a cache, which either returns an already-loaded value for a given key
        /// or atomically computes or retrieves it using the supplied CacheLoader. If another 
        /// thread is currently loading the value for this key, simply waits for that thread 
        /// to finish and returns its loaded value. Note that multiple threads can concurrently 
        /// load values for distinct keys.</para>
        /// <para>This method does not alter the state of this CacheBuilder instance, so it can be invoked 
        /// again to create multiple independent caches.</para>
        /// </summary>
        /// <param name="loader">the cache loader used to obtain new values</param>
        /// <returns>a cache having the requested features</returns>
        public LoadingCache<K, V> Build(CacheLoader<K, V> loader)
        {
            return new LoadingCache<K, V>(_maximumSize)
            {
                Loader = loader
            };
        }
    }
}
