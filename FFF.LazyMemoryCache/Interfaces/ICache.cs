using FFF.LazyMemoryCache.Enum;
using Microsoft.Extensions.Caching.Memory;
using System.Collections;

namespace FFF.LazyMemoryCache;

/// <summary>
/// Defines a set of methods for caching operations. This interface abstracts cache management functionalities,
/// allowing for implementation details to vary without affecting consumers of the cache. It supports generic types
/// for flexibility and includes both synchronous and asynchronous methods.
/// </summary>
public interface ICache
{
    /// <summary>
    ///     Define the number of seconds to cache objects for by default
    /// </summary>
    CacheDefaults DefaultCachePolicy { get; }

    /// <summary>
    /// Adds an item of type T to the cache under a specified key, applying the provided cache entry options.
    /// </summary>
    /// <typeparam name="T">The type of the item to be cached.</typeparam>
    /// <param name="key">The key under which the item will be stored.</param>
    /// <param name="item">The item to store in the cache.</param>
    /// <param name="policy">Cache entry options that dictate the behavior of the cache item, such as expiration.</param>
    void Add<T>(string key, T item, MemoryCacheEntryOptions policy);

    /// <summary>
    /// Retrieves all keys of type T currently stored in the cache.
    /// </summary>
    /// <typeparam name="T">The type of the keys to retrieve.</typeparam>
    /// <returns>An enumerable collection of all cache keys of type T.</returns>
    IEnumerable<T> GetKeys<T>();

    /// <summary>
    /// Retrieves all cache keys regardless of their type.
    /// </summary>
    /// <returns>A collection of all cache keys.</returns>
    ICollection GetKeys();

    /// <summary>
    /// Retrieves a cached item of type T using a specified key.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key associated with the cache item.</param>
    /// <returns>The cached item if it exists; otherwise, null.</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Asynchronously retrieves a cached item of type T using a specified key.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key associated with the cache item.</param>
    /// <returns>A task that, when completed, returns the cached item if it exists; otherwise, null.</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Attempts to retrieve a cached item of type T by its key, returning a value that indicates whether the item was found.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="value">When this method returns, contains the cached item of type T if the key is found; otherwise, the default value for type T.</param>
    /// <returns>true if the item was retrieved successfully; otherwise, false.</returns>
    bool TryGetValue<T>(string key, out T? value);

    /// <summary>
    /// Retrieves a cached item of type T by its key or adds it if it doesn't exist using a provided factory method.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory method to create the item if it is not found in the cache.</param>
    /// <returns>The cached or newly added item.</returns>
    T? GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory);

    /// <summary>
    /// Retrieves a cached item of type T by its key or adds it if it doesn't exist, applying a specified cache entry policy.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory method to create the item if it is not found in the cache.</param>
    /// <param name="policy">The cache entry options to apply if the item is added.</param>
    /// <returns>The cached or newly added item.</returns>
    T? GetOrAdd<T>(string key, Func<ICacheEntry, T> addItemFactory, MemoryCacheEntryOptions? policy);

    /// <summary>
    /// Removes a cached item identified by a specified key.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    void Remove(string key);

    /// <summary>
    /// Removes cached items whose keys start with a specified prefix.
    /// </summary>
    /// <param name="key">The prefix to match for removal.</param>
    /// <param name="mode"> RemoveMode</param>
    void Remove(string key, RemoveMode mode);

    /// <summary>
    /// Removes cached items that meet a specified predicate condition applied to their keys.
    /// </summary>
    /// <param name="key">The key used in the predicate to evaluate which items to remove.</param>
    /// <param name="predicate">A function to test each key for a condition that determines removal.</param>
    void Remove(string key, Func<string, bool> predicate);

    /// <summary>
    /// Asynchronously retrieves a cached item of type T by its key or adds it if it doesn't exist, using a provided factory method.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory method to create the item if it is not found in the cache.</param>
    /// <returns>A task that, when completed, returns the cached or newly added item.</returns>
    Task<T?> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory);

    /// <summary>
    /// Asynchronously retrieves a cached item of type T by its key or adds it if it doesn't exist, applying a specified cache entry policy.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory method to create the item if it is not found in the cache.</param>
    /// <param name="policy">The cache entry options to apply if the item is added.</param>
    /// <returns>A task that, when completed, returns the cached or newly added item.</returns>
    Task<T?> GetOrAddAsync<T>(string key, Func<ICacheEntry, Task<T>> addItemFactory, MemoryCacheEntryOptions? policy);
}

