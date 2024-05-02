using Microsoft.Extensions.Caching.Memory;

namespace FFF.LazyMemoryCache;

/// <summary>
/// Enumeration representing different modes of removing strings.
/// </summary>
public enum RemoveMode
{
    /// <summary>
    /// Remove strings that start with the given substring, ignoring case.
    /// </summary>
    StartWithIgnoreCase,

    /// <summary>
    /// Remove strings that start with the given substring.
    /// </summary>
    StartWith,

    /// <summary>
    /// Remove strings that end with the given substring.
    /// </summary>
    EndWith,

    /// <summary>
    /// Remove strings that end with the given substring, ignoring case.
    /// </summary>
    EndWithIgnoreCase,

    /// <summary>
    /// Remove strings that are exactly equal to the given substring.
    /// </summary>
    Equal,

    /// <summary>
    /// Remove strings that are exactly equal to the given substring, ignoring case.
    /// </summary>
    EqualIgnoreCase,

    /// <summary>
    /// Remove strings that contain the given substring.
    /// </summary>
    Contain
}

