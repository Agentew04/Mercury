using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SAAE.Editor.Models.Compilation;

namespace SAAE.Editor.Services;

// TODO: should this be an interfcae?

/// <summary>
/// An interface for a service that has the ability to compile assembly code
/// into an executable. Derivative classes may compile assembly for different architectures
/// or even C# (roslyn) and C/C++ (bundled compiler)
/// </summary>
/// <remarks>This api doesnt support additional compilations yet.</remarks>
public interface ICompilerService {

    public ValueTask<CompilationResult> CompileStandaloneAsync(Stream input);

    public ValueTask<CompilationResult> CompileAsync(CompilationInput input);
}