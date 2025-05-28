namespace SAAE.Engine.Memory;

/// <summary>
/// Defines common methods for memory access.
/// </summary>
public interface IMemory {

    /// <summary>
    /// The endianness of this memory. I.e. how are the bytes
    /// ordered in a word.
    /// </summary>
    public Endianess Endianess { get; }
    
    #region Single Value Methods

    byte ReadByte(ulong address);

    void WriteByte(ulong address, byte value);

    int ReadWord(ulong address);

    void WriteWord(ulong address, int value);
    
    #endregion

    #region Buffer Methods

    byte[] Read(ulong address, int length);

    void Write(ulong address, byte[] bytes);
    
    void Read(ulong address, Span<byte> bytes);

    void Write(ulong address, Span<byte> bytes);

    void Read(ulong address, Span<int> words);
    
    void Write(ulong address, Span<int> words);

    #endregion

}
