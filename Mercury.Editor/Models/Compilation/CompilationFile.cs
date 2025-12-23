using System.IO;
using Mercury.Editor.Extensions;

namespace Mercury.Editor.Models.Compilation;

/// <summary>
/// Represents a file that is part of a compilation.
/// </summary>
public struct CompilationFile
{
    /// <summary>
    /// Creates a new structure for a file that will be compiled.
    /// </summary>
    /// <param name="filepath">The path of the file relative to <see cref="ProjectFile.ProjectDirectory"/></param>
    /// <param name="entryPoint">If this file is an entry point for the program.</param>
    public CompilationFile(PathObject filepath, bool entryPoint = false)
    {
        Path = filepath;
        IsEntryPoint = entryPoint;
    }
    
    /// <summary>
    /// The absolute path of the file to be compiled. Cannot be relative because it can't differentiate
    /// between project files and stdlib files(live in shared directory).
    /// </summary>
    public PathObject Path { get; private set; }

    /// <summary>
    /// The hash of the contents of this file. It is calculated by
    /// <see cref="CalculateHash"/>.
    /// </summary>
    public byte[] Hash { get; private set; } = [];
    
    /// <summary>
    /// Defines if this file is an entry point for the program or not.
    /// </summary>
    public bool IsEntryPoint { get; private set; }

    /// <summary>
    /// Calculates the hash of the contents of this file.
    /// </summary>
    public void CalculateHash()
    {
        using FileStream stream = File.OpenRead(Path.ToString());
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] hash = sha256.ComputeHash(stream);
        Hash = hash;
    }
}