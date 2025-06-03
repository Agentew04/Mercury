namespace SAAE.Engine.Memory.Cache;

/// <summary>
/// A hybrid cache between <see cref="DirectAccessCache"/> and <see cref="FullyAssociativeCache"/>.
/// </summary>
public class SetAssociativeCache : ICache
{
    private readonly IMemory backingMemory;
    private readonly int blockSize;
    private readonly int blockCount;
    private readonly int associativity;
    private readonly int tagSize;
    private readonly int indexSize;
    private readonly int skipSize;
    private readonly List<CacheSet> lines;
    
    // TODO: adaptar isso para ser SetAssociative
    // private readonly Queue<uint>? fifoBlockQueue;
    // private readonly List<uint>? lruTimestamps;
    // private uint lruHead;
    // private uint lfuAccessCounter;
    // private const uint LfuHalflife = 64;
    // private readonly List<uint>? lfuFrequencies;
    // private readonly Random? rng;

    public Endianess Endianess => backingMemory.Endianess;
    public CacheWritePolicy WritePolicy { get; }
    /// <inheritdoc cref="FullyAssociativeCache.SubstitutionStrategy"/>
    public SubstitutionStrategy SubstitutionStrategy { get; }
    
    /// <summary>
    /// Creates a new hybrid cache with the specified parameters.
    /// </summary>
    /// <param name="backingMemory">The underlying memory device</param>
    /// <param name="blockSize">The size in bytes of the data stored in each block. Must be a power of 2</param>
    /// <param name="blockCount">The amount of rows this cache will have. Must be a power of 2</param>
    /// <param name="associativity">The amount of blocks each how has.</param>
    /// <param name="writePolicy">The rule which dictates when a modified cache block will be written to
    /// the backing memory</param>
    /// <param name="substitutionStrategy">Which algorithm to use when evicting a block from a row</param>
    /// <exception cref="ArgumentException">Thrown when blockSize, blockCount or associativity are invalid</exception>
    public SetAssociativeCache(IMemory backingMemory, int blockSize, int blockCount, int associativity,
        CacheWritePolicy writePolicy, SubstitutionStrategy substitutionStrategy)
    {
        this.backingMemory = backingMemory;
        WritePolicy = writePolicy;
        this.blockSize = blockSize;
        this.blockCount = blockCount;
        this.associativity = associativity;
        SubstitutionStrategy = substitutionStrategy;
        
        if (blockSize <= 0 || blockCount <= 0 || associativity <= 0)
        {
            throw new ArgumentException("Block size, block count and associativity must be greater than zero.");
        }
        
        // check if blockSize is a power of two
        if ((blockSize & (blockSize - 1)) != 0)
        {
            throw new ArgumentException("Block size must be a power of two.");
        }
        
        // check if blockCount is a power of two
        if ((blockCount & (blockCount - 1)) != 0)
        {
            throw new ArgumentException("Block count must be a power of two.");
        }
        
        indexSize = (int)Math.Log2(blockCount);
        skipSize = (int)Math.Log2(blockSize);
        tagSize = sizeof(ulong) - indexSize - skipSize;
        
        switch (substitutionStrategy)
        {
            // case SubstitutionStrategy.Fifo:
            //     fifoBlockQueue = new Queue<uint>(blockCount);
            //     break;
            // case SubstitutionStrategy.Lru:
            //     lruTimestamps = new List<uint>(blockCount);
            //     for (int i = 0; i < blockCount; i++) {
            //         lruTimestamps.Add(0);
            //     }
            //     break;
            // case SubstitutionStrategy.Lfu:
            //     lfuFrequencies = new List<uint>(blockCount);
            //     for (int i = 0; i < blockCount; i++) {
            //         lfuFrequencies.Add(0);
            //     }
            //     break;
            // case SubstitutionStrategy.Random:
            //     rng = new Random();
            //     break;
            default:
                throw new NotSupportedException("SubstitutionStrategy not supported.");
        }

        lines = [];
        for (int i = 0; i < blockCount; i++)
        {
            CacheSet set = new()
            {
                Blocks = new CacheBlock[associativity],
            };
            for (int j = 0; j < associativity; j++)
            {
                CacheBlock block = new()
                {
                    Valid = false,
                    Modified = false,
                    Tag = 0,
                    Data = new byte[blockSize]
                };
                set.Blocks[j] = block;
            }
            lines.Add(set);
        }
    }
    
    public event EventHandler<CacheMissEventArgs>? OnCacheMiss;
    public event EventHandler<CacheEvictionEventArgs>? OnCacheEvict;

    private (ulong tag, int index) GetAddressData(ulong address)
    {
        ulong tag = address >> (indexSize + skipSize);
        ulong indexMask = ((1UL << indexSize) - 1) << skipSize; // (1<<n)-1 is 0xFF..FF with n bits set to 1
        int index = (int)((address & indexMask) >> skipSize);
        return (tag, index);
    }

    private bool IsHit(ulong address, out int line, out int column)
    {
        (ulong tag, int index) = GetAddressData(address);
        CacheSet set = lines[index];

        line = index;
        // Check if the block is in the cache
        for (int i = 0; i < associativity; i++)
        {
            CacheBlock block = set.Blocks[i];
            if (block.Valid && block.Tag == tag)
            {
                column = i;
                
                // switch (SubstitutionStrategy)
                // {
                //     case SubstitutionStrategy.Lru:
                //         lruTimestamps![index] = lruHead++;
                //         break;
                //     case SubstitutionStrategy.Lfu:
                //         lfuFrequencies![index]++;
                //         if(lfuAccessCounter++ >= LfuHalflife) {
                //             // halve the frequencies
                //             for (int j = 0; i < lfuFrequencies!.Count; i++) {
                //                 lfuFrequencies[j] /= 2;
                //             }
                //             lfuAccessCounter = 0;
                //         }
                //         break;
                // }
                
                return true; // Cache hit
            }
        }

        line = -1;
        column = -1;
        return false; // Cache miss
    }

    private (int line, int column) LoadBlock(ulong address)
    {
        (ulong tag, int line) = GetAddressData(address);
        
        CacheSet set = lines[line];

        // check if there is a free block in the set
        bool found = false;
        int candidate = -1;
        for (int i = 0; i < associativity; i++)
        {
            if (!set.Blocks[i].Valid)
            {
                candidate = i;
                found = true;
                break;
            }
        }

        if (!found)
        {
            // evict someone
            int evictedIndex = GetEvictedBlockIndex(set);
            ulong evictedTag = set.Blocks[evictedIndex].Tag;
            ulong addressEvicted = (evictedTag << (indexSize + skipSize)) |
                ((ulong)line << skipSize);
            OnCacheEvict?.Invoke(this, new CacheEvictionEventArgs(address, addressEvicted));
            StoreBlock(set.Blocks[evictedIndex], evictedIndex);
            
            candidate = evictedIndex;
        }
        
        // load at candidate index
        CacheBlock block = set.Blocks[candidate];
        block.Valid = true;
        block.Modified = false; // Reset modified flag when loading a new block
        block.Tag = tag;
        backingMemory.Read(address, block.Data);
        return (line, candidate);
    }
    
    private int GetEvictedBlockIndex(CacheSet set)
    {
        // TODO: fazer isso
        // Use the substitution strategy to determine which block to evict
        switch (SubstitutionStrategy)
        {
            case SubstitutionStrategy.Fifo:
                return -1;
            case SubstitutionStrategy.Lru:
                return -1;
            case SubstitutionStrategy.Lfu:
                return -1;
            case SubstitutionStrategy.Random:
                Random random = new();
                return random.Next(0, associativity);
            default:
                throw new NotSupportedException($"SubstitutionStrategy {SubstitutionStrategy} is not supported.");
        }
    }
    
    private byte GetByteFromBlock(ulong address, int line, int column) {
        // Calculate the offset within the block
        int offset = (int)(address % (ulong)blockSize);
        if (offset < 0 || offset >= blockSize) {
            throw new ArgumentOutOfRangeException(nameof(address), "Address is out of bounds for the block size.");
        }
        // Return the byte from the block's data
        return lines[line].Blocks[column].Data[offset];
    }
    
    private void StoreBlock(CacheBlock block, int index)
    {
        if (WritePolicy != CacheWritePolicy.WriteBack || !block.Modified)
        {
            return;
        }
        // Write the block back to the backing memory
        ulong address = (block.Tag << (indexSize + skipSize)) | ((ulong)index << skipSize);
        backingMemory.Write(address, block.Data);
        block.Tag = 0;
        block.Valid = false;
        block.Modified = false; // Reset modified flag after writing
    }
    
    #region Memory

    public byte ReadByte(ulong address)
    {
        if (!IsHit(address, out int line, out int column))
        {
            // miss
            OnCacheMiss?.Invoke(this, new CacheMissEventArgs(address));
            
            // load block from backing memory
            (line, column) = LoadBlock(address);
        }
        
        // hit
        return GetByteFromBlock(address, line, column);
    }

    public void WriteByte(ulong address, byte value)
    {
        if (!IsHit(address, out int line, out int column))
        {
            // miss
            OnCacheMiss?.Invoke(this, new CacheMissEventArgs(address));
            
            // load block from backing memory
            (line, column) = LoadBlock(address);
        }
        
        // hit
        CacheBlock block = lines[line].Blocks[column];
        if (WritePolicy == CacheWritePolicy.WriteThrough)
        {
            backingMemory.WriteByte(address, value);
            block.Data[address % (ulong)blockSize] = value;
        }
        else
        {
            // write back
            block.Data[address % (ulong)blockSize] = value;
            block.Modified = true;
        }
    }

    public int ReadWord(ulong address)
    {
        throw new NotImplementedException();
    }

    public void WriteWord(ulong address, int value)
    {
        throw new NotImplementedException();
    }

    public byte[] Read(ulong address, int length)
    {
        throw new NotImplementedException();
    }

    public void Write(ulong address, byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public void Read(ulong address, Span<byte> bytes)
    {
        throw new NotImplementedException();
    }

    public void Write(ulong address, Span<byte> bytes)
    {
        throw new NotImplementedException();
    }

    public void Read(ulong address, Span<int> words)
    {
        throw new NotImplementedException();
    }

    public void Write(ulong address, Span<int> words)
    {
        throw new NotImplementedException();
    }
    
    #endregion
    
    public void Dispose()
    {
        if (WritePolicy != CacheWritePolicy.WriteBack)
        {
            return;
        }
        // para cada bloco modificado, escrever de volta na memoria
        for(int i=0;i<lines.Count;i++)
        {
            for(int j=0;j<associativity;j++)
            {
                CacheBlock block = lines[i].Blocks[j];
                if(!block.Valid || !block.Modified) continue;   
                StoreBlock(block, i);
            }
        }
        GC.SuppressFinalize(this);
    }

    private class CacheSet
    {
        public required CacheBlock[] Blocks { get; init; }
    }

    private class CacheBlock
    {
        public bool Valid { get; set; }
        public bool Modified { get; set; }
        public ulong Tag { get; set; }
        public required byte[] Data { get; init; }
    }
}