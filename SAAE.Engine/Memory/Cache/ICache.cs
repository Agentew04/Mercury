namespace SAAE.Engine.Memory.Cache;

/// <summary>
/// Common interface that all caches must implement.
/// </summary>
public interface ICache : IMemory, IDisposable {

    /// <summary>
    /// The WritePolicy of this cache. Can be either
    /// <see cref="CacheWritePolicy.WriteThrough"/> or <see cref="CacheWritePolicy.WriteBack"/>.
    /// Defines when the written data is commited to the backing memory.
    /// </summary>
    public CacheWritePolicy WritePolicy { get; }
    
    /// <summary>
    /// Event raised when an access results in a cache miss.
    /// </summary>
    event EventHandler<CacheMissEventArgs>? OnCacheMiss;
    
    /// <summary>
    /// Event raised when a block is evicted from the cache to open
    /// up space for a new block.
    /// </summary>
    event EventHandler<CacheEvictionEventArgs>? OnCacheEvict;
}

public class CacheMissEventArgs : EventArgs {
    public ulong Address { get; }

    public CacheMissEventArgs(ulong address) {
        Address = address;
    }
}

public class CacheEvictionEventArgs : EventArgs {
    public ulong Address { get; }
    public ulong EvictedAddress { get; }

    public CacheEvictionEventArgs(ulong address, ulong evictedAddress) {
        Address = address;
        EvictedAddress = evictedAddress;
    }
}