using SAAE.Engine.Mips.Runtime.OS;

namespace SAAE.Engine.Common;

public struct OperatingSystemType {
    
    public Type OsType { get; set; }
    
    public string Name { get; set; }
    
    public Architecture CompatibleArchitecture { get; set; }
}

/// <summary>
/// Class that provides a simple interface for the UI to get available operating systems
/// and its metadata.
/// </summary>
public static class OperatingSystemManager {

    private static readonly List<OperatingSystemType> AvailableOs = [];
    
    static OperatingSystemManager() {
        Register<Mars>();
        Register<MockLinux>();
        // Registrar novos sistemas operacionais abaixo
    }

    private static void Register<T>() where T : IOperatingSystem, new() {
        T os = new();
        var osType = new OperatingSystemType {
            OsType = typeof(T),
            Name = os.FriendlyName,
            CompatibleArchitecture = os.CompatibleArchitecture
        };
        AvailableOs.Add(osType);
        os.Dispose();
    }
    
    public static IEnumerable<OperatingSystemType> GetAvailableOperatingSystems() {
        return AvailableOs;
    }
    
}