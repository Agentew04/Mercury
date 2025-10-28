using System.Buffers.Binary;
using System.Numerics;
using System.Linq;

namespace Mercury.Engine.Memory.Cache;

/// <summary>
/// Basic cache that uses the direct access architecture.
/// </summary>
public class DirectAccessCache : ICache {

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

    public CacheWritePolicy WritePolicy { get; init; }
    public Endianess Endianess => backingMemory.Endianess;
    
    
    /// <summary>
    /// Creates a new cache with the specified memory, block count, and block size.
    /// </summary>
    /// <param name="backingMemory">The backing memory</param>
    /// <param name="blockCount">The amount of blocks this cache has</param>
    /// <param name="blockSize">The amount of bytes each block has</param>
    /// <param name="writePolicy">The write policy of this cache</param>
    public DirectAccessCache(IMemory backingMemory, int blockCount, int blockSize, CacheWritePolicy writePolicy) {
        this.backingMemory = backingMemory;
        this.blockCount = blockCount;
        this.blockSize = blockSize;
        WritePolicy = writePolicy;
        
        // check if block size is power of 2
        if(blockSize <= 0 || (blockSize & (blockSize - 1)) != 0) {
            throw new ArgumentException("Block size must be a multiple of 4 bytes.");
        }
        
        // check if block count is a power of 2
        if ((blockCount & (blockCount - 1)) != 0 || blockCount <= 0) {
            throw new ArgumentException("Block count must be a power of 2 and greater than zero.");
        }

        cacheBlocks = [];
        for(int i=0;i<blockCount;i++) {
            cacheBlocks.Add(new CacheBlock {
                Valid = false,
                Modified = false,
                Tag = -1,
                Data = new byte[blockSize]
            });
        }
    }

    private readonly List<CacheBlock> cacheBlocks;

    private (int tag, int index) GetAddressData(ulong address) {
        //4 bytes -> 2 bit skip
        // n bytes -> skip = n / 4
        int skip = BitOperations.Log2((uint)blockSize);
        
        // Calculate the index and tag from the address
        int indexSize = BitOperations.Log2((uint)blockCount);
        int tagSize = sizeof(ulong)*8 - indexSize - skip;
        ulong tagMask = (unchecked((ulong)-1) >> (indexSize+skip)) << (indexSize+skip);
        ulong indexMask = (unchecked((ulong)-1) >> (tagSize+skip)) << skip;
        int index = (int)((address & indexMask) >> skip);
        int tag = (int)((address & tagMask) >> (indexSize + skip));
        return (tag, index);
    }

    private class CacheBlock {
        public bool Valid { get; set; }
        public bool Modified { get; set; }
        public int Tag { get; set; }
        public required byte[] Data { get; init; }
    }
        
    public event EventHandler<CacheMissEventArgs>? OnCacheMiss;
    public event EventHandler<CacheEvictionEventArgs>? OnCacheEvict;
    
    private ulong hitCount = 0;
    private ulong missCount = 0;
    private ulong evictionCount = 0;
    
    public CacheStatistics GetStatistics() {
        return new CacheStatistics() {
            Hits = hitCount,
            Misses = missCount,
            Evictions = evictionCount
        };
    }

    private void LoadBlock(ulong address) {
        (int tag, int index) = GetAddressData(address);
        
        Span<byte> data = stackalloc byte[blockSize];
        // Read the block from memory
        backingMemory.Read(address, data);
        // Update the cache block; check modified if write policy is WriteBack
        if (cacheBlocks[index].Valid)
        {
            int indexSize = BitOperations.Log2((uint)blockCount);
            int skip = BitOperations.Log2((uint)blockSize);
            // ja havia algo aqui, atira evento de evict
            evictionCount++;
            OnCacheEvict?.Invoke(this, 
                new CacheEvictionEventArgs(
                    address, 
                    evictedAddress: (ulong)cacheBlocks[index].Tag << (indexSize + skip) 
                                    | (ulong)index << skip
            ));
        }
        if (WritePolicy == CacheWritePolicy.WriteBack && cacheBlocks[index].Valid && cacheBlocks[index].Modified) {
            StoreBlock(cacheBlocks[index], index);
        }
        // Update the cache block
        cacheBlocks[index].Valid = true;
        cacheBlocks[index].Modified = false;
        cacheBlocks[index].Tag = tag;
        data.CopyTo(cacheBlocks[index].Data);
    }

    private void StoreBlock(CacheBlock block, int blockIndex)
    {
        ulong tag = (ulong)block.Tag << (BitOperations.Log2((uint)blockCount) + BitOperations.Log2((uint)blockSize));
        ulong index = (ulong)blockIndex << BitOperations.Log2((uint)blockSize);
        ulong address = tag | index;
        // skip is null because it indexes inside the block
        backingMemory.Write(address, block.Data);
    }
    
    private byte GetByteFromBlock(CacheBlock block, ulong address) {
        // Calculate the offset within the block
        int offset = (int)(address % (ulong)blockSize);
        if (offset < 0 || offset >= blockSize) {
            throw new ArgumentOutOfRangeException(nameof(address), "Address is out of bounds for the block size.");
        }
        // Return the byte from the block's data
        return block.Data[offset];
    }

    private bool IsHit(int tag, int index) {
        // Check if the cache block is valid and the tag matches
        return cacheBlocks[index].Valid && cacheBlocks[index].Tag == tag;
    }
    
    #region IMemory


    public byte ReadByte(ulong address) {
        (int tag, int index) = GetAddressData(address);

        if (!cacheBlocks[index].Valid) {
            // nao ha nada na cache, miss
            missCount++;
            OnCacheMiss?.Invoke(this, new CacheMissEventArgs(address));
            //load
            LoadBlock(address);
        }
        else if (cacheBlocks[index].Tag != tag) {
            // ha algo na cache, mas nao eh o que queremos, miss
            missCount++;
            OnCacheMiss?.Invoke(this, new CacheMissEventArgs(address));
            //load
            LoadBlock(address);
        }
        else {
            hitCount++;
        }
        return GetByteFromBlock(cacheBlocks[index], address);
    }

    public void WriteByte(ulong address, byte value) {
        (int tag, int index) = GetAddressData(address);
        if (WritePolicy == CacheWritePolicy.WriteThrough) {
            if (IsHit(tag, index)) {
                hitCount++;
                cacheBlocks[index].Data[(int)(address % (ulong)blockSize)] = value;
                backingMemory.Write(address, cacheBlocks[index].Data);
                return;
            }
            // miss
            missCount++;
            OnCacheMiss?.Invoke(this, new CacheMissEventArgs(address));
            // load block
            LoadBlock(address);
            // write on cache and memory
            cacheBlocks[index].Data[(int)(address % (ulong)blockSize)] = value;
            backingMemory.Write(address, cacheBlocks[index].Data);
        }
        else {
            // WriteBack
            if (IsHit(tag, index)) {
                hitCount++;
                cacheBlocks[index].Data[(int)(address % (ulong)blockSize)] = value;
                cacheBlocks[index].Modified = true;
                return;
            }
            // miss
            missCount++;
            OnCacheMiss?.Invoke(this, new CacheMissEventArgs(address));
            LoadBlock(address);
            cacheBlocks[index].Data[(int)(address % (ulong)blockSize)] = value;
            cacheBlocks[index].Modified = true;
        }
    }

    public int ReadWord(ulong address) {
        Span<byte> data = stackalloc byte[4];
        data[0] = ReadByte(address);
        data[1] = ReadByte(address + 1);
        data[2] = ReadByte(address + 2);
        data[3] = ReadByte(address + 3);

        return Endianess switch {
            Endianess.LittleEndian => BinaryPrimitives.ReadInt32LittleEndian(data),
            Endianess.BigEndian => BinaryPrimitives.ReadInt32BigEndian(data),
            _ => throw new NotSupportedException("Unsupported endianness.")
        };
    }

    public void WriteWord(ulong address, int value) {
        Span<byte> data = stackalloc byte[4];
        if (Endianess == Endianess.LittleEndian) {
            BinaryPrimitives.WriteInt32LittleEndian(data, value);
        }
        else if (Endianess == Endianess.BigEndian) {
            BinaryPrimitives.WriteInt32BigEndian(data, value);
        }
        else {
            throw new NotSupportedException("Unsupported endianness.");
        }

        WriteByte(address, data[0]);
        WriteByte(address + 1, data[1]);
        WriteByte(address + 2, data[2]);
        WriteByte(address + 3, data[3]);
    }

    public byte[] Read(ulong address, int length) {
        if (length <= 0) {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");
        }

        byte[] result = new byte[length];
        for (int i = 0; i < length; i++) {
            result[i] = ReadByte(address + (ulong)i);
        }
        return result;
    }

    public void Write(ulong address, byte[] bytes) {
        if (bytes == null) {
            throw new ArgumentNullException(nameof(bytes), "Bytes cannot be null.");
        }
        if (bytes.Length <= 0) {
            throw new ArgumentOutOfRangeException(nameof(bytes), "Bytes length must be greater than zero.");
        }

        for (int i = 0; i < bytes.Length; i++) {
            WriteByte(address + (ulong)i, bytes[i]);
        }
    }

    public void Read(ulong address, Span<byte> bytes) {
        if (bytes == null || bytes.Length <= 0) {
            throw new ArgumentOutOfRangeException(nameof(bytes), "Bytes span must be greater than zero.");
        }

        for (int i = 0; i < bytes.Length; i++) {
            bytes[i] = ReadByte(address + (ulong)i);
        }
    }

    public void Write(ulong address, Span<byte> bytes) {
        if (bytes == null || bytes.Length <= 0) {
            throw new ArgumentOutOfRangeException(nameof(bytes), "Bytes span must be greater than zero.");
        }

        for (int i = 0; i < bytes.Length; i++) {
            WriteByte(address + (ulong)i, bytes[i]);
        }
    }

    public void Read(ulong address, Span<int> words) {
        if (words == null || words.Length <= 0) {
            throw new ArgumentOutOfRangeException(nameof(words), "Words span must be greater than zero.");
        }

        for (int i = 0; i < words.Length; i++) {
            words[i] = ReadWord(address + (ulong)(i * 4));
        }
    }

    public void Write(ulong address, Span<int> words) {
        if (words == null || words.Length <= 0) {
            throw new ArgumentOutOfRangeException(nameof(words), "Words span must be greater than zero.");
        }

        for (int i = 0; i < words.Length; i++) {
            WriteWord(address + (ulong)(i * 4), words[i]);
        }
    }

    #endregion

    public void Dispose()
    {
        if (WritePolicy != CacheWritePolicy.WriteBack)
        {
            return;
        }
        // para cada bloco modificado, escrever de volta na memoria
        for(int i=0;i<cacheBlocks.Count;i++)
        {
            CacheBlock block = cacheBlocks[i];
            if(!block.Valid || !block.Modified) continue;   
            StoreBlock(cacheBlocks[i], i);
        }
        GC.SuppressFinalize(this);
    }
}