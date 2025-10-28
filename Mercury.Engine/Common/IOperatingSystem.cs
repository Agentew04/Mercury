using Mercury.Engine.Common;

namespace Mercury.Engine;

/// <summary>
/// Base interface for all operating systems across
/// all architectures.
/// </summary>
public interface IOperatingSystem : IDisposable {
    
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
    
    /// <summary>
    /// A weak reference to the machine that this operating system is installed.
    /// </summary>
    public WeakReference<Machine?> Machine { get; set; }
}