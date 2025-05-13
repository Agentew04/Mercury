using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

    /// <summary>
    /// Calculates a unique id from an ordered set of hashes.
    /// </summary>
    /// <returns>A unique identifier of this collection of hashes</returns>
    public Guid CalculateId(string? entryPointPrefix)
    {
        List<byte[]> hashes = Files.Select(x =>
        {
            if (x.Hash.Length > 0)
            {
                return x.Hash;
            }
            x.CalculateHash(entryPointPrefix);
            return x.Hash;
        }).ToList();
        
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        using var ms = new MemoryStream();
        foreach (byte[] hash in hashes)
        {
            ms.Write(hash, 0, hash.Length);
        }
        ms.Seek(0, SeekOrigin.Begin);
        byte[] id = sha256.ComputeHash(ms).Take(16).ToArray();
        return new Guid(id);
    }
}