using System.Collections.Generic;
using System.IO;

namespace SAAE.Editor.Models.Compilation;

/// <summary>
/// Structure that organizes the input to the compilation process.
/// </summary>
public readonly struct CompilationInput
{
    /// <summary>
    /// A list with all files that will be compiled.
    /// </summary>
    public List<CompilationFile> Files { get; init; }
}