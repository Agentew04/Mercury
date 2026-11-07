namespace Mercury.Engine.Common;

/// <summary>
/// Base interface for all operating systems across
/// all architectures.
/// </summary>
public interface ISyscallModule : IDisposable, IModule {
    
    /// <summary>
    /// The target architecture that this operating system
    /// accepts.
    /// </summary>
    public Architecture CompatibleArchitecture { get; }
    
    /// <summary>
    /// The user-friendly name of this operating system. 
    /// </summary>
    /// <remarks>
    /// This string should not need to be localized. It
    /// is a name after all.
    /// </remarks>
    public string FriendlyName { get; }
    
    /// <summary>
    /// It is a unique string identifier used to serialize
    /// operating systems in files.
    /// </summary>
    public string Identifier { get; }
}