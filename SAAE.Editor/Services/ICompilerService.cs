using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ELFSharp.ELF;

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

/// <summary>
/// A structure with information about the result
/// of a compilation process.
/// </summary>
public readonly struct CompilationResult
{
    /// <summary>
    /// Whether the compilation was successful or not.
    /// </summary>
    public bool IsSuccess { get; init; }

    public CompilationError Error { get; init; }
    
    /// <summary>
    /// A stream to the binary output of the compilation process.
    /// </summary>
    public Stream? OutputStream { get; init; }
    
    /// <summary>
    /// The output processed as an ELF file.
    /// </summary>
    public IELF? OutputElf { get; init; }
}

public enum CompilationError {
    None,
    InternalError,
    TimeoutError,
    FileNotFound,
    CompilationError,
    LinkingError,
}

/// <summary>
/// Structure that organizes the input to the compilation process.
/// </summary>
public readonly struct CompilationInput
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