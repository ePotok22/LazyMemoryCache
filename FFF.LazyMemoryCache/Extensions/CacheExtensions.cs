using FFF.LazyMemoryCache.Enum;
using Microsoft.Extensions.Caching.Memory;

namespace FFF.LazyMemoryCache;

/// <summary>
/// Static class for cache extension methods that simplify common caching operations.
/// </summary>
public static class CacheExtensions
{
    /// <summary>
    /// Adds an item to the cache using the default cache policy.
    /// </summary>
    /// <typeparam name="T">The type of the item to add to the cache.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The key under which to store the item.</param>
    /// <param name="item">The item to store.</param>
    public static void Add<T>(this ICache cache, string key, T item)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        cache.Add(key, item, cache.DefaultCachePolicy.BuildCacheOptions());
    }

    /// <summary>
    /// Adds an item to the cache with an absolute expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the item to add.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="item">The item to store.</param>
    /// <param name="expires">The absolute expiration date and time for the cache entry.</param>
    public static void Add<T>(this ICache cache, string key, T item, DateTimeOffset expires)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        cache.Add(key, item, new MemoryCacheEntryOptions { AbsoluteExpiration = expires });
    }

    /// <summary>
    /// Adds an item to the cache with a sliding expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the item to add.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="item">The item to store.</param>
    /// <param name="slidingExpiration">The sliding expiration timespan after which the cache entry expires if it is not accessed.</param>
    public static void Add<T>(this ICache cache, string key, T item, TimeSpan slidingExpiration)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        cache.Add(key, item, new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration });
    }

    /// <summary>
    /// Retrieves or adds an item to the cache using the default cache policy.
    /// </summary>
    /// <typeparam name="T">The type of the item to get or add.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory function to create the item if it does not exist in the cache.</param>
    /// <returns>The cached or newly added item.</returns>
    public static T? GetOrAdd<T>(this ICache cache, string key, Func<T> addItemFactory)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        return cache.GetOrAdd(key, addItemFactory, cache.DefaultCachePolicy.BuildCacheOptions());
    }

    /// <summary>
    /// Retrieves or adds an item to the cache with an absolute expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the item to get or add.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory function to create the item if it does not exist.</param>
    /// <param name="expires">The absolute expiration date and time for the cache entry.</param>
    /// <returns>The cached or newly added item.</returns>
    public static T? GetOrAdd<T>(this ICache cache, string key, Func<T> addItemFactory, DateTimeOffset expires)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        return cache.GetOrAdd(key, addItemFactory, new MemoryCacheEntryOptions { AbsoluteExpiration = expires });
    }

    /// <summary>
    /// Retrieves or adds an item to the cache with an absolute expiration time and a specific expiration mode.
    /// </summary>
    /// <typeparam name="T">The type of the item to get or add.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory function to create the item if it does not exist.</param>
    /// <param name="expires">The absolute expiration date and time for the cache entry.</param>
    /// <param name="mode">The expiration mode, determining how expiration is handled.</param>
    /// <returns>The cached or newly added item.</returns>
    public static T? GetOrAdd<T>(this ICache cache, string key, Func<T> addItemFactory, DateTimeOffset expires, ExpirationMode mode)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));

        switch (mode)
        {
            case ExpirationMode.LazyExpiration:
                return cache.GetOrAdd(key, addItemFactory, new MemoryCacheEntryOptions { AbsoluteExpiration = expires });
            default:
                return cache.GetOrAdd(key, addItemFactory, new LazyMemoryCacheEntryOptions().SetAbsoluteExpiration(expires, mode));
        }
    }

    /// <summary>
    /// Retrieves or adds an item to the cache with a sliding expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the item to get or add.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory function to create the item if it does not exist.</param>
    /// <param name="slidingExpiration">The sliding expiration timespan after which the cache entry expires if it is not accessed.</param>
    /// <returns>The cached or newly added item.</returns>
    public static T? GetOrAdd<T>(this ICache cache, string key, Func<T> addItemFactory, TimeSpan slidingExpiration)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        return cache.GetOrAdd(key, addItemFactory, new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration });
    }

    /// <summary>
    /// Retrieves or adds an item to the cache with specified caching options.
    /// </summary>
    /// <typeparam name="T">The type of the item to get or add.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory function to create the item if it does not exist in the cache.</param>
    /// <param name="policy">The caching options to apply for the item in the cache.</param>
    /// <returns>The cached or newly added item.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the cache parameter is null.</exception>
    public static T? GetOrAdd<T>(this ICache cache, string key, Func<T> addItemFactory, MemoryCacheEntryOptions policy)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        return cache.GetOrAdd(key, _ => addItemFactory(), policy);
    }

    /// <summary>
    /// Asynchronously retrieves or adds an item to the cache using the default cache policy.
    /// </summary>
    /// <typeparam name="T">The type of the item to get or add.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory function to create the item if it does not exist in the cache, executed asynchronously.</param>
    /// <returns>A task representing the asynchronous operation, with the result being the cached or newly added item.</returns>
    public static Task<T?> GetOrAddAsync<T>(this ICache cache, string key, Func<Task<T>> addItemFactory)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        return cache.GetOrAddAsync(key, addItemFactory, cache.DefaultCachePolicy.BuildCacheOptions());
    }

    /// <summary>
    /// Asynchronously retrieves or adds an item to the cache with an absolute expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the item to get or add.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory function to create the item if it does not exist, executed asynchronously.</param>
    /// <param name="expires">The absolute expiration date and time for the cache entry.</param>
    /// <returns>A task representing the asynchronous operation, with the result being the cached or newly added item.</returns>
    public static Task<T?> GetOrAddAsync<T>(this ICache cache, string key, Func<Task<T>> addItemFactory, DateTimeOffset expires)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        return cache.GetOrAddAsync(key, addItemFactory, new MemoryCacheEntryOptions { AbsoluteExpiration = expires });
    }

    /// <summary>
    /// Asynchronously retrieves or adds an item to the cache with an absolute expiration time and a specific expiration mode.
    /// </summary>
    /// <typeparam name="T">The type of the item to get or add.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory function to create the item if it does not exist, executed asynchronously.</param>
    /// <param name="expires">The absolute expiration date and time for the cache entry.</param>
    /// <param name="mode">The expiration mode, determining how expiration is handled.</param>
    /// <returns>A task representing the asynchronous operation, with the result being the cached or newly added item.</returns>
    public static Task<T?> GetOrAddAsync<T>(this ICache cache, string key, Func<Task<T>> addItemFactory, DateTimeOffset expires, ExpirationMode mode)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));

        switch (mode)
        {
            case ExpirationMode.LazyExpiration:
                return cache.GetOrAddAsync(key, addItemFactory, new MemoryCacheEntryOptions { AbsoluteExpiration = expires });
            default:
                return cache.GetOrAddAsync(key, addItemFactory, new LazyMemoryCacheEntryOptions().SetAbsoluteExpiration(expires, mode));
        }
    }

    /// <summary>
    /// Asynchronously retrieves or adds an item to the cache with a sliding expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the item to get or add.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory function to create the item if it does not exist, executed asynchronously.</param>
    /// <param name="slidingExpiration">The sliding expiration timespan after which the cache entry expires if it is not accessed.</param>
    /// <returns>A task representing the asynchronous operation, with the result being the cached or newly added item.</returns>
    public static Task<T?> GetOrAddAsync<T>(this ICache cache, string key, Func<Task<T>> addItemFactory, TimeSpan slidingExpiration)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        return cache.GetOrAddAsync(key, addItemFactory, new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration });
    }

    /// <summary>
    /// Asynchronously retrieves or adds an item to the cache with specified caching options.
    /// </summary>
    /// <typeparam name="T">The type of the item to get or add.</typeparam>
    /// <param name="cache">The cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="addItemFactory">A factory function, executed asynchronously, to create the item if it does not exist in the cache.</param>
    /// <param name="policy">The caching options to apply for the item in the cache.</param>
    /// <returns>A task representing the asynchronous operation, with the result being the cached or newly added item.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the cache parameter is null.</exception>
    public static Task<T?> GetOrAddAsync<T>(this ICache cache, string key, Func<Task<T>> addItemFactory, MemoryCacheEntryOptions policy)
    {
        if (cache == null) throw new ArgumentNullException(nameof(cache));
        return cache.GetOrAddAsync(key, _ => addItemFactory(), policy);
    }
}