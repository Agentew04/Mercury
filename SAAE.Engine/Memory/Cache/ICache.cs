namespace SAAE.Engine.Memory.Cache;

/// <summary>
/// Common interface that all caches must implement.
/// </summary>
public interface ICache : IMemory {

    /// <summary>
    /// The WritePolicy of this cache. Can be either
    /// <see cref="CacheWritePolicy.WriteThrough"/> or <see cref="CacheWritePolicy.WriteBack"/>.
    /// Defines when the written data is commited to the backing memory.
    /// </summary>
    public CacheWritePolicy WritePolicy { get; }
    
    /// <summary>
    /// Event raised when this cache misses a access.
    /// </summary>
    event EventHandler<CacheMissEventArgs>? OnCacheMiss;
}

public class CacheMissEventArgs : EventArgs {
    public ulong Address { get; }

    public CacheMissEventArgs(ulong address) {
        Address = address;
    }
}