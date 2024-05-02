using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace FFF.LazyMemoryCache;

/// <summary>
/// Provides extensions for the IMemoryCache interface to access private members that contain cache entries.
/// This class uses reflection to create delegates for accessing non-public fields and properties,
/// allowing retrieval of cache keys that are otherwise inaccessible due to encapsulation in the underlying implementation.
/// </summary>
internal static class MemoryCacheExtensions
{
    const string COHERENTSTATE = "CoherentState";
    const string UNDERCOHERENTSTATE = "_coherentState";
    const string UNDERENTRIES = "_entries";

    // Lazy initialization for retrieving the "_coherentState" field from an IMemoryCache instance.
    // The field is expected to contain the actual state of the cache, including all entries.
    private static readonly Lazy<Func<MemoryCache, object>> GetCoherentState = new Lazy<Func<MemoryCache, object>>(() =>
        CreateGetter<MemoryCache, object>(GetFieldInfo<MemoryCache>(UNDERCOHERENTSTATE)));

    // Lazy initialization for retrieving the "_entries" field from the coherent state object.
    // This dictionary holds all cache entries, allowing access to cache keys.
    private static readonly Lazy<Func<object, IDictionary>> InternalGetEntries = new Lazy<Func<object, IDictionary>>(() =>
        CreateGetter<object, IDictionary>(GetFieldInfo(GetNestedType<MemoryCache>(COHERENTSTATE), UNDERENTRIES)));

    /// <summary>
    /// Retrieves FieldInfo for a specific field within a type. Throws an exception if the field does not exist.
    /// This method is critical for the reflection used in delegate creation, ensuring that only existing fields are accessed.
    /// </summary>
    /// <typeparam name="T">The type containing the field.</typeparam>
    /// <param name="fieldName">The name of the field to find.</param>
    /// <returns>The FieldInfo for the specified field.</returns>
    private static FieldInfo GetFieldInfo<T>(string fieldName)
    {
        var field = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
            throw new InvalidOperationException($"Field '{fieldName}' not found in type '{typeof(T)}'.");
        return field;
    }

    /// <summary>
    /// Retrieves FieldInfo for a specific field within a specified type. Throws an exception if the field does not exist.
    /// </summary>
    /// <param name="type">The type containing the field.</param>
    /// <param name="fieldName">The name of the field to find.</param>
    /// <returns>The FieldInfo for the specified field.</returns>
    private static FieldInfo GetFieldInfo(Type type, string fieldName)
    {
        var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
            throw new InvalidOperationException($"Field '{fieldName}' not found in type '{type.FullName}'.");
        return field;
    }

    /// <summary>
    /// Retrieves a nested type by its name within a specified type, throwing an exception if the nested type is not found.
    /// This method supports accessing complex internal structures within cached objects.
    /// </summary>
    /// <typeparam name="T">The type that contains the nested type.</typeparam>
    /// <param name="nestedTypeName">The name of the nested type to find.</param>
    /// <returns>The Type of the nested type.</returns>
    private static Type GetNestedType<T>(string nestedTypeName)
    {
        var type = typeof(T).GetNestedType(nestedTypeName, BindingFlags.NonPublic);
        if (type == null)
            throw new InvalidOperationException($"Nested type '{nestedTypeName}' not found in type '{typeof(T)}'.");
        return type;
    }

    /// <summary>
    /// Creates a dynamic method to generate a delegate capable of accessing a private field within an object.
    /// This method underpins the ability to access private cache data structures, which are crucial for retrieving cache keys.
    /// </summary>
    /// <typeparam name="TParam">The type of the object that contains the field.</typeparam>
    /// <typeparam name="TReturn">The type of the field's value.</typeparam>
    /// <param name="field">The field information used to generate the accessor.</param>
    /// <returns>A delegate that provides access to the field.</returns>
    private static Func<TParam, TReturn> CreateGetter<TParam, TReturn>(FieldInfo field)
    {
        var methodName = $"{field.ReflectedType!.FullName}.get_{field.Name}";
        var method = new DynamicMethod(methodName, typeof(TReturn), new[] { typeof(TParam) }, true);
        var ilGen = method.GetILGenerator();
        ilGen.Emit(OpCodes.Ldarg_0);
        ilGen.Emit(OpCodes.Ldfld, field);
        ilGen.Emit(OpCodes.Ret);
        return (Func<TParam, TReturn>)method.CreateDelegate(typeof(Func<TParam, TReturn>));
    }

    // A composite delegate that combines the functions of getting the coherent state and then the entries from that state.
    // This forms the primary method of accessing all cache entries directly.
    private static readonly Func<MemoryCache, IDictionary> GetEntries =
        cache => InternalGetEntries.Value(GetCoherentState.Value(cache));

    /// <summary>
    /// Retrieves all keys from an IMemoryCache instance as an ICollection.
    /// This is the primary interface method that abstracts all the complexity of accessing private fields behind a simple API.
    /// </summary>
    /// <param name="memoryCache">The IMemoryCache instance from which to retrieve keys.</param>
    /// <returns>An ICollection containing all cache keys.</returns>
    public static ICollection GetKeys(this IMemoryCache memoryCache) =>
        GetEntries((MemoryCache)memoryCache).Keys;

    /// <summary>
    /// Generic method to retrieve all keys from an IMemoryCache instance, cast to a specific type.
    /// This method provides type-specific access to cache keys, enhancing usability in strongly typed scenarios.
    /// </summary>
    /// <typeparam name="T">The type to which the keys should be cast.</typeparam>
    /// <param name="memoryCache">The IMemoryCache instance from which to retrieve keys.</param>
    /// <returns>An IEnumerable containing all keys cast to the specified type.</returns>
    public static IEnumerable<T> GetKeys<T>(this IMemoryCache memoryCache) =>
        memoryCache.GetKeys().OfType<T>();
}
