using System.Threading.Tasks;

namespace SAAE.Editor.Services;

/// <summary>
/// An interface for a service that has the ability to compile assembly code
/// into an executable
/// </summary>
public interface ICompilerService {
    public Task<bool> CompileAssembly(string assemblyCode);
}