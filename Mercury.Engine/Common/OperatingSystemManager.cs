using System.Diagnostics.CodeAnalysis;
using Mercury.Engine.Mips.Runtime.OS;

namespace Mercury.Engine.Common;

public struct OperatingSystemType : IEquatable<OperatingSystemType> {
    
    public Type OsType { get; set; }
    
    public string Name { get; set; }
    
    public string Identifier { get; set; }
    
    public Architecture CompatibleArchitecture { get; set; }

    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is not OperatingSystemType type) {
            return false;
        }
        return Name == type.Name && CompatibleArchitecture == type.CompatibleArchitecture
                                 && OsType.Name == type.OsType.Name
                                 && Identifier == type.Identifier;
    }

    public bool Equals(OperatingSystemType other) {
        return OsType.Equals(other.OsType) && Name == other.Name && Identifier == other.Identifier && CompatibleArchitecture == other.CompatibleArchitecture;
    }

    public override int GetHashCode() {
        return HashCode.Combine(OsType, Name, Identifier, (int)CompatibleArchitecture);
    }
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
            Identifier = os.Identifier,
            CompatibleArchitecture = os.CompatibleArchitecture
        };
        AvailableOs.Add(osType);
        os.Dispose();
    }
    
    public static IEnumerable<OperatingSystemType> GetAvailableOperatingSystems() {
        return AvailableOs;
    }
    
}