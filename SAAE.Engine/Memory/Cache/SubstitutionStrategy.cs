namespace SAAE.Engine.Memory.Cache;

/// <summary>
/// Represents a substitution strategy for cache row/block replacement.
/// </summary>
public enum SubstitutionStrategy {
    /// <summary>
    /// Discards the oldest block in the cache.
    /// </summary>
    Fifo,
    /// <summary>
    /// Discards the least recently used block in the cache.
    /// </summary>
    /// <remarks>It is a perfect LRU</remarks>
    Lru,
    /// <summary>
    /// Discards the least frequently used block in the cache.
    /// </summary>
    Lfu,
    /// <summary>
    /// Discards a random block in the cache.
    /// </summary>
    Random,
}