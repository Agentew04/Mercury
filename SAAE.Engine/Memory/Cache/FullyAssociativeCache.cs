namespace SAAE.Engine.Memory.Cache;

/// <summary>
/// Represents a fully associative cache. There are no index bits, so any block can be placed in any row.
/// </summary>
public class FullyAssociativeCache : ICache {
    
    /// <summary>
    /// The backing memory for this cache.
    /// </summary>
    private readonly IMemory backingMemory;
    /// <summary>
    /// The amount of blocks this cache has.
    /// </summary>
    private readonly int blockCount;
    /// <summary>
    /// The amount of bytes each block has.
    /// </summary>
    private readonly int blockSize;

    /// <summary>
    /// The write policy of this cache.
    /// </summary>
    public CacheWritePolicy WritePolicy { get; init; }

    public event EventHandler<CacheMissEventArgs>? OnCacheMiss;

    public SubstitutionStrategy SubstitutionStrategy { get; init; }

    public Endianess Endianess => backingMemory.Endianess;
    

    public FullyAssociativeCache(IMemory backingMemory, int blockCount, int blockSize, CacheWritePolicy writePolicy, SubstitutionStrategy substitutionStrategy) {
        this.backingMemory = backingMemory;
        WritePolicy = writePolicy;
        SubstitutionStrategy = substitutionStrategy;
        this.blockCount = blockCount;
        this.blockSize = blockSize;
    }

    #region Memory Access

    public byte ReadByte(ulong address) {
        throw new NotImplementedException();
    }

    public void WriteByte(ulong address, byte value) {
        throw new NotImplementedException();
    }

    public int ReadWord(ulong address) {
        throw new NotImplementedException();
    }

    public void WriteWord(ulong address, int value) {
        throw new NotImplementedException();
    }

    public byte[] Read(ulong address, int length) {
        throw new NotImplementedException();
    }

    public void Write(ulong address, byte[] bytes) {
        throw new NotImplementedException();
    }

    public void Read(ulong address, Span<byte> bytes) {
        throw new NotImplementedException();
    }

    public void Write(ulong address, Span<byte> bytes) {
        throw new NotImplementedException();
    }

    public void Read(ulong address, Span<int> words) {
        throw new NotImplementedException();
    }

    public void Write(ulong address, Span<int> words) {
        throw new NotImplementedException();
    }

    #endregion
}