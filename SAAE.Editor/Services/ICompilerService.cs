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

    /// <summary>
    /// Compiles the given input into an executable. The input is a list of
    /// the files to be compiled.
    /// </summary>
    /// <remarks>One of the <see cref="CompilationFile"/> in <see cref="input"/> must
    /// have <see cref="CompilationFile.IsEntryPoint"/> set to true.</remarks>
    /// <param name="input">The collection of files to be compiled</param>
    /// <returns>Returns a collection of information about the compilation</returns>
    public ValueTask<CompilationResult> CompileAsync(CompilationInput input);
    
    /// <summary>
    /// A cache with the result of the last compilation.
    /// </summary>
    public CompilationResult LastCompilationResult { get; }
}