using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ELFSharp.ELF;

namespace SAAE.Editor.Services;

/// <summary>
/// An interface for a service that has the ability to compile assembly code
/// into an executable. Derivative classes may compile assembly for different architectures
/// or even C# (roslyn) and C/C++ (bundled compiler)
/// </summary>
public abstract class CompilerService {

    public async ValueTask<CompilationResult> CompileStandaloneAsync(Stream input) 
    {
        return await ValueTask.FromResult<CompilationResult>(default);
    }

    public async ValueTask<CompilationResult> CompileAsync(CompilationInput input)
    {
        return await ValueTask.FromResult<CompilationResult>(default);
    }

    protected abstract Task Compile();
}

/// <summary>
/// A structure with information about the result
/// of a compilation process.
/// </summary>
public struct CompilationResult
{
    /// <summary>
    /// Whether the compilation was successful or not.
    /// </summary>
    public bool IsSuccess { get; init; }
}


public struct CompilationInput
{
    /// <summary>
    /// A list with all files that will be compiled.
    /// </summary>
    public List<Stream> Files { get; init; }
    /// <summary>
    /// The index of the entry point in the list of files.
    /// </summary>
    public int EntryPoint { get; init; }
}