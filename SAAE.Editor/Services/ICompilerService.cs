using System.Threading.Tasks;
using ELFSharp.ELF;

namespace SAAE.Editor.Services;

/// <summary>
/// An interface for a service that has the ability to compile assembly code
/// into an executable
/// </summary>
public interface ICompilerService {
    public Task<(bool success, IELF? elf)> TryCompileAssemblyAsync(string assemblyCode);

    public Task<bool> TryCompileCodeAsync(string highlevelCode);
}