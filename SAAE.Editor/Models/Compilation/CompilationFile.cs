using System;
using System.IO;
using SAAE.Editor.Services;

namespace SAAE.Editor.Models.Compilation;

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
    public CompilationFile(string filepath, bool entryPoint = false)
    {
        FilePath = filepath;
        IsEntryPoint = entryPoint;
    }
    
    /// <summary>
    /// The relative path of this file. Relative to <see cref="ProjectFile.ProjectDirectory"/>.
    /// </summary>
    public string FilePath { get; private set; }

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
    /// <param name="baseDirectory">The base directory of the program.</param>
    public void CalculateHash(string baseDirectory)
    {
        var fullPath = Path.Combine(baseDirectory, FilePath);
        using var stream = File.OpenRead(fullPath);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(stream);
        Hash = hash;
    }
}