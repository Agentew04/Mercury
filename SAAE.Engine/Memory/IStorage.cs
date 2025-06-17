namespace SAAE.Engine.Memory;

/// <summary>
/// A storage interface that the 
/// virtual memory can use to store
/// unloaded pages.
/// </summary>
internal interface IStorage : IDisposable {

    /// <summary>
    /// Writes a <see cref="Page"/> to the
    /// secondary memory.
    /// </summary>
    /// <param name="page">The page structure to write</param>
    void WritePage(Page page);

    /// <summary>
    /// Reads a <see cref="Page"/> from
    /// the secondary memory.
    /// </summary>
    /// <param name="pageNumber">The number of the page to read</param>
    /// <returns>The page read</returns>
    Page ReadPage(int pageNumber);
}

/// <summary>
/// Represents the type of storage that a memory uses.
/// </summary>
public enum StorageType
{
    FileOriginal,
    FileOptimized,
    Volatile
}