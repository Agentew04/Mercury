using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        FullPath = filepath;
        IsEntryPoint = entryPoint;
    }
    
    /// <summary>
    /// The absolute path of the file to be compiled. Cannot be relative because it can't differentiate
    /// between project files and stdlib files(live in shared directory).
    /// </summary>
    public string FullPath { get; private set; }

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
    /// <param name="entryPointPrefix"></param>
    public void CalculateHash(string? entryPointPrefix)
    {
        if (IsEntryPoint)
        {
            ArgumentNullException.ThrowIfNull(entryPointPrefix);
            
            using MemoryStream ms = new();
            StreamWriter writer = new(ms, leaveOpen: true);
            writer.Write(entryPointPrefix);
            writer.Close();
            Stream fs = File.OpenRead(FullPath);
            fs.CopyTo(ms);
            fs.Close();
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            ms.Seek(0, SeekOrigin.Begin);
            byte[] hash = sha256.ComputeHash(ms);
            ms.Close();
            Hash = hash;
        }
        else
        {
            using FileStream stream = File.OpenRead(FullPath);
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hash = sha256.ComputeHash(stream);
            Hash = hash;
        }   
    }
}