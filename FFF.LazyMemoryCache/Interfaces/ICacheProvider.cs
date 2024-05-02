using Microsoft.Extensions.Caching.Memory;
using System.Collections;

namespace FFF.LazyMemoryCache;

/// <summary>
/// Defines the contract for cache providers which manage caching operations.
/// </summary>
public interface ICacheProvider : IDisposable
{
    /// <summary>
    /// Retrieves all keys of a specified type from the cache.
    /// </summary>
    /// <typeparam name="T">The type of keys to retrieve.</typeparam>
    /// <returns>An enumerable containing all keys in the cache of the specified type.</returns>
    IEnumerable<T> GetKeys<T>();

    /// <summary>
    /// Retrieves all keys from the cache, regardless of their type.
    /// </summary>
    /// <returns>A collection containing all keys in the cache.</returns>
    ICollection GetKeys();

    /// <summary>
    /// Tries to retrieve a value from the cache based on a key.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key associated with the cache item.</param>
    /// <param name="value">Outputs the cached item if it exists; otherwise, the default value for the type.</param>
    /// <returns>True if the item exists in the cache; otherwise, false.</returns>
    bool TryGetValue<T>(string key, out T? value);

    /// <summary>
    /// Retrieves an item from the cache by its key.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key of the cache item.</param>
    /// <returns>The cached item if it exists; otherwise, null.</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Sets or updates an item in the cache under a specified key with a defined policy.
    /// </summary>
    /// <typeparam name="T">The type of the item to store.</typeparam>
    /// <param name="key">The key under which to store the item.</param>
    /// <param name="value">The item to store in the cache.</param>
    /// <param name="policy">Cache entry options specifying behavior like expiration.</param>
    void Set<T>(string key, T value, MemoryCacheEntryOptions? policy = null);

    /// <summary>
    /// Retrieves an item from the cache by its key or creates it using a function if it doesn't exist.
    /// This method ensures thread-safe creation and retrieval of cache entries.
    /// </summary>
    /// <typeparam name="T">The type of the item to be retrieved or created.</typeparam>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="factory">The function to execute to create the item if it is not found.</param>
    /// <param name="policy">Cache entry options specifying behavior like expiration.</param>
    /// <returns>The cached or newly created item.</returns>
    T? GetOrCreate<T>(string key, Func<ICacheEntry, T> factory, MemoryCacheEntryOptions? policy = null);

    /// <summary>
    /// Removes an item from the cache based on its key.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    void Remove(string key);
}
