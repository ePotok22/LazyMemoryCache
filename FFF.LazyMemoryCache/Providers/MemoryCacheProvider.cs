using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Collections;

namespace FFF.LazyMemoryCache;

/// <summary>
/// Provides a thread-safe caching mechanism to store and retrieve data,
/// utilizing a ReaderWriterLockSlim for concurrency control to allow multiple
/// readers or exclusive writer access.
/// </summary>
internal class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    /// <summary>
    /// Initializes a new instance of the MemoryCacheProvider class.
    /// </summary>
    /// <param name="cache">The memory cache instance to use for caching operations.</param>
    public MemoryCacheProvider(IMemoryCache cache) =>
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));

    /// <summary>
    /// Retrieves all keys of a specified type from the cache.
    /// </summary>
    /// <typeparam name="T">The type of keys to retrieve.</typeparam>
    /// <returns>An enumerable containing all keys of the specified type.</returns>
    public IEnumerable<T> GetKeys<T>() => _cache.GetKeys<T>();

    /// <summary>
    /// Retrieves all keys from the cache, regardless of their type.
    /// </summary>
    /// <returns>A collection containing all keys in the cache.</returns>
    public ICollection GetKeys() => _cache.GetKeys();

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key from the cache.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <param name="value">When this method returns, contains the object from the cache
    /// associated with the specified key, if the key is found; otherwise, null.</param>
    /// <returns>true if the cache contains an element with the specified key; otherwise, false.</returns>
    public bool TryGetValue<T>(string key, out T? value) => _cache.TryGetValue(key, out value);

    /// <summary>
    /// Retrieves an item from the cache by its key.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key of the item to retrieve.</param>
    /// <returns>The cached item if found; otherwise, null.</returns>
    public T? Get<T>(string key)
    {
        _lock.EnterReadLock();
        try
        {
            _cache.TryGetValue(key, out T? value);
            return value;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Adds an item to the cache or updates an existing item within the cache.
    /// </summary>
    /// <typeparam name="T">The type of the item to cache.</typeparam>
    /// <param name="key">The key for the cache item.</param>
    /// <param name="value">The item to cache.</param>
    /// <param name="policy">The cache entry options, or null to use default policy.</param>
    public void Set<T>(string key, T value, MemoryCacheEntryOptions? policy = null)
    {
        _lock.EnterWriteLock();
        try
        {
            _cache.Set(key, value, policy);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Removes the item associated with the specified key from the cache.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    public void Remove(string key)
    {
        _lock.EnterWriteLock();
        try
        {
            _cache.Remove(key);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Retrieves an item from the cache by its key or creates it using the specified factory
    /// if the item does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve or create.</typeparam>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="factory">A delegate to create the item if it does not exist in the cache.</param>
    /// <param name="policy">The cache entry options to use if creating a new item, or null for default options.</param>
    /// <returns>The cached or newly created item.</returns>
    public T? GetOrCreate<T>(string key, Func<ICacheEntry, T> factory, MemoryCacheEntryOptions? policy = null)
    {
        T? value;
        _lock.EnterUpgradeableReadLock();
        try
        {
            if (_cache.TryGetValue(key, out value))
                return value;

            _lock.EnterWriteLock();
            try
            {
                if (!_cache.TryGetValue(key, out value)) // Double-check to minimize the window for a race condition
                {
                    using (ICacheEntry entry = _cache.CreateEntry(key))
                    {
                        value = factory(entry);
                        entry.SetValue(value);
                        if (policy != null)
                            entry.SetOptions(policy);
                        _cache.Set(key, value, policy);
                    }
                }
                return value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Releases all resources used by the MemoryCacheProvider.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the MemoryCacheProvider and optionally releases
    /// the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _lock?.Dispose();
            _cache?.Dispose();
        }
    }
}
