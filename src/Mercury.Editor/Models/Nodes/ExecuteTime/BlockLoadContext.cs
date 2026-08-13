using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Mercury.Editor.Models.Nodes.ExecuteTime;

public class BlockLoadContext : AssemblyLoadContext {
    
    private readonly AssemblyDependencyResolver resolver;
    
    public BlockLoadContext(string mainAssemblyPath) : base(isCollectible: true) {
        resolver = new AssemblyDependencyResolver(mainAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName) {
        Assembly? loaded = Default.Assemblies
            .FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
        if (loaded != null) {
            return loaded;
        }
        string? path = resolver.ResolveAssemblyToPath(assemblyName);
        return path != null ? LoadFromAssemblyPath(path) : null;
    }
}