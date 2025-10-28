namespace SAAE.Engine.Memory.Cache;

/// <summary>
/// A data structure that holds generic statistics about cache performance.
/// </summary>
public struct CacheStatistics {
    
    /// <summary>
    /// The raw amount of hits the cache has had.
    /// </summary>
    public ulong Hits { get; set; }
    
    /// <summary>
    /// The raw amount of misses the cache has had.
    /// </summary>
    public ulong Misses { get; set; }
    
    /// <summary>
    /// The amount of block evictions that have occurred in the cache.
    /// </summary>
    public ulong Evictions { get; set; }
    
    /// <summary>
    /// A percentage value representing the hit rate of the cache. Goes from 0 to 1.
    /// </summary>
    public double HitRate => Hits + Misses == 0 ? 0 : (double)Hits / (Hits + Misses);
    
    /// <summary>
    /// A percentage value representing the miss rate of the cache. Goes from 0 to 1.
    /// </summary>
    public double MissRate => Hits + Misses == 0 ? 0 : (double)Misses / (Hits + Misses);
}