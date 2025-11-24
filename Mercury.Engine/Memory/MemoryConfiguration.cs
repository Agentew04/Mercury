namespace Mercury.Engine.Memory;

public readonly struct MemoryConfiguration {
    private const ulong Kb = 1024;
    private const ulong Mb = 1024 * Kb;
    private const ulong Gb = 1024 * Mb;

    /// <summary>
    /// The total amount of bytes of this memory.
    /// </summary>
    public ulong Size { get; init; } = 4*Gb;

    /// <summary>
    /// The size of each page.
    /// </summary>
    public ulong PageSize { get; init; } = 4*Kb;

    /// <summary>
    /// The maximum amount of loaded pages(frames) in memory.
    /// </summary>
    public uint MaxLoadedPages { get; init; } = 4096;

    /// <summary>
    /// Wether or not to collect runtime debug information of
    /// the use of the memory.
    /// </summary>
    public bool CollectDebugInfo { get; init; } = false;

    /// <summary>
    /// Path to the file where pages are stored when they are
    /// unloaded.
    /// </summary>
    public required string ColdStoragePath { get; init; }

    /// <summary>
    /// Selects the type of storage that will be used. If <see cref="StorageType.FileOptimized"/>,
    /// only created pages will be stored on the cold storage file.
    /// If <see cref="StorageType.FileOriginal"/>, all custom file structure will not be used and
    /// a raw version of the memory will be used.
    /// If <see cref="StorageType.Volatile"/>, no cold storage will be used and all generated pages
    /// will be kept on RAM.
    /// </summary>
    /// <remarks>
    /// Beware of using this option as <see cref="StorageType.FileOriginal"/> alongside
    /// a big number for <see cref="Size"/> as it will create a file with
    /// the same size.
    /// </remarks>
    public StorageType StorageType { get; init; } = StorageType.FileOptimized;

    public bool ForceColdStorageReset { get; init; } = false;
    
    public Endianess Endianess { get; init; } = Endianess.LittleEndian;

    public MemoryConfiguration() {}
}