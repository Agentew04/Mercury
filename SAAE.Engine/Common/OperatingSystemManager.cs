namespace SAAE.Engine;

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
        List<Type> availableTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IOperatingSystem).IsAssignableFrom(p) && !p.IsAbstract)
            .ToList();

        foreach (Type type in availableTypes) {
            if(Activator.CreateInstance(type) is not IOperatingSystem os) {
                continue;
            }

            var osType = new OperatingSystemType {
                OsType = type,
                Name = os.FriendlyName,
                CompatibleArchitecture = os.CompatibleArchitecture
            };
            AvailableOs.Add(osType);
            os.Dispose();
        }
    }
    
    public static IEnumerable<OperatingSystemType> GetAvailableOperatingSystems() {
        return AvailableOs;
    }
    
}