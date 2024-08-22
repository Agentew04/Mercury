namespace SAAE.Engine.Memory;

internal readonly struct VirtualMemoryConfiguration {
    private const ulong KB = 1024;
    private const ulong MB = 1024 * KB;
    private const ulong GB = 1024 * MB;

    /// <summary>
    /// The total amount of bytes of this virtual memory.
    /// </summary>
    public ulong Size { get; init; } = 4*GB;

    /// <summary>
    /// The size of each page.
    /// </summary>
    public ulong PageSize { get; init; } = 4*KB;

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
    /// If true, only created pages will be stored on the cold storage
    /// file. If false, all custom file structure will not be used and
    /// a raw version of the memory will be used.
    /// </summary>
    /// <remarks>
    /// Beware of using this option as <see langword="false"/> alongside
    /// a big number for <see cref="Size"/> as it will create a file with
    /// the same size.
    /// </remarks>
    public bool ColdStorageOptimization { get; init; } = true;

    public VirtualMemoryConfiguration() {}
}