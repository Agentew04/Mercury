using System.Numerics;
using Mercury.Engine.Memory;
using Mercury.Engine.Memory.Cache;

namespace Mercury.Engine.Common.Builders;

/// <summary>
/// A builder for creating cache instances.
/// </summary>
public class CacheBuilder : IBuilder<ICache>
{
    // TODO: Implement properties for cache configuration, such as size, eviction policy, etc.

    private int associativity = 1;
    private int blockCount = 64;
    private int blockSize = 4;
    private SubstitutionStrategy strategy = SubstitutionStrategy.Fifo;
    private CacheWritePolicy writePolicy = CacheWritePolicy.WriteThrough;
    private IMemory? backingMemory;
    
    public CacheBuilder WithAssociativity(int associativity)
    {
        this.associativity = associativity;
        return this;
    }

    public CacheBuilder WithBlockCount(int blockCount)
    {
        this.blockCount = blockCount;
        return this;
    }
    
    public CacheBuilder WithBlockSize(int blockSize)
    {
        this.blockSize = blockSize;
        return this;
    }
    
    public CacheBuilder WithSubstitutionStrategy(SubstitutionStrategy strategy)
    {
        this.strategy = strategy;
        return this;
    }

    public CacheBuilder WithWritePolicy(CacheWritePolicy policy)
    {
        writePolicy = policy;
        return this;
    }

    public CacheBuilder BasedOn(IMemory memory)
    {
        backingMemory = memory;
        return this;
    }
    
    public ICache Build()
    {
        if (backingMemory is null)
        {
            throw new InvalidOperationException("Backing memory must be set before building the cache.");
        }
        
        if (associativity == 1)
        {
            // Direct Mapped Cache
            return new DirectAccessCache(backingMemory, blockSize, blockSize, writePolicy);
        }else if (blockCount == 1)
        {
            // fully associative cache
            return new FullyAssociativeCache(backingMemory, associativity, blockSize, writePolicy,
                strategy);
        }
        else
        {
            // hybrid cache
            throw new NotSupportedException("Not yet supported.");
        }
        return null!;
    }
}