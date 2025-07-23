using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SAAE.Editor.Extensions;

public static class PathExtensions {

    /// <summary>
    /// Creates a object that represents a directory path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>The object representing the path</returns>
    public static PathObject ToDirectoryPath(this string path) {
        bool root = Path.IsPathFullyQualified(path) || Path.IsPathRooted(path);

        char[] delims = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];
        string[] entries = path.Split(delims, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return new PathObject {
            Filename = string.Empty,
            IsAbsolute = root,
            IsDirectory = true,
            IsFile = false,
            Parts = [..entries],
            Extension = string.Empty
        };
    }

    /// <summary>
    /// Creates a file path object from its string representation.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>The object representing the path</returns>
    public static PathObject ToFilePath(this string path) {
        if (path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)) {
            throw new NotSupportedException("A filepath cant end with a directory separator");
        }
        
        int lastIndex = path.LastIndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
        string file = Path.GetFileName(path);
        if (lastIndex == -1) {
            return "".ToDirectoryPath().File(file);
        }
        PathObject folder = path[..lastIndex].ToDirectoryPath();
        return folder.File(file);
    }
}

/// <summary>
/// Object that represents a path. Can be a file or a directory.
/// </summary>
/// <remarks>Normally you create one with <see cref="PathExtensions.ToDirectoryPath"/>
/// or <see cref="PathExtensions.ToFilePath"/>.</remarks>
public readonly struct PathObject : IXmlSerializable, IEquatable<PathObject> {
    
    /// <summary>
    /// The folder parts of this path.
    /// </summary>
    public required ImmutableArray<string> Parts { get; init; }
    
    /// <summary>
    /// Wether this path is rooted or is relative.
    /// </summary>
    public required bool IsAbsolute { get; init; }
    
    /// <summary>
    /// Wether this path is a directory or not.
    /// </summary>
    public required bool IsDirectory { get; init; }
    
    /// <summary>
    /// Wether this path is a file or not.
    /// </summary>
    public required bool IsFile { get; init; }
    
    /// <summary>
    /// The filename without the extension.
    /// </summary>
    public required string Filename { get; init; }
    
    /// <summary>
    /// The extension including the dot. 
    /// </summary>
    public required string Extension { get; init; }
    
    /// <summary>
    /// Returns the file name with extension.
    /// </summary>
    public string FullFileName => Filename + Extension;

    /// <summary>
    /// Returns the string representation of this path.
    /// </summary>
    /// <remarks>On relative paths: if the platform is linux, adds a preceding <see cref="System.IO.Path.DirectorySeparatorChar"/>.
    /// On Windows it does not because first elemento of <see cref="Parts"/> is a drive letter.</remarks>
    /// <returns></returns>
    public override string ToString() {
        StringBuilder sb = new();
        if (IsAbsolute) {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()) {
                sb.Append(System.IO.Path.DirectorySeparatorChar);
            } 
        }

        foreach(string part in Parts) {
            sb.Append(part);
            sb.Append(System.IO.Path.DirectorySeparatorChar);
        }

        if (IsFile) {
            sb.Append(FullFileName);
        }

        return sb.ToString();
    }
    
    public bool Equals(PathObject? other) {
        return other is not null && Equals(other.Value);
    }

    /// <summary>
    /// Creates a new path with a folder appended at the end.
    /// </summary>
    /// <param name="newPart">The name of the new folder</param>
    /// <returns>The new path</returns>
    /// <exception cref="NotSupportedException">Thrown if the current path is a file</exception>
    public PathObject Folder(string newPart) => Folders(newPart);
    
    /// <summary>
    /// Appends an ordered collection of new folders. 
    /// </summary>
    /// <param name="newParts">The folders to append</param>
    /// <returns>The new path</returns>
    /// <exception cref="NotSupportedException">Thrown if the current path is a file</exception>
    public PathObject Folders(params string[] newParts) {
        if (!IsDirectory || IsFile) {
            throw new NotSupportedException("Cannot append new folder on a file");
        }
        ImmutableArray<string> parts2 = Parts.AddRange(newParts);
        return new PathObject {
            Filename = string.Empty,
            Parts = parts2,
            IsFile = false,
            IsDirectory = true,
            IsAbsolute = IsAbsolute,
            Extension = string.Empty
        };
    }

    /// <summary>
    /// Appends two paths together.
    /// </summary>
    /// <param name="other">The path to append to the right side</param>
    /// <returns>The new path</returns>
    /// <exception cref="NotSupportedException">Thrown if the left side is a file or the right side is absolute.</exception>
    public PathObject Append(PathObject other) {
        if (other.IsAbsolute) {
            throw new NotSupportedException("Cannot append a rooted path to another");
        }

        if (IsFile) {
            throw new NotSupportedException("Cannot append a path to a file");
        }

        PathObject newfolder = Folders(other.Parts.ToArray());
        return other.IsFile ? newfolder.File(other.FullFileName) : newfolder;
    }

    /// <summary>
    /// Appends two files.
    /// </summary>
    public static PathObject operator +(PathObject lhs, PathObject rhs) => lhs.Append(rhs);
    public static bool operator ==(PathObject lhs, PathObject rhs) => lhs.Equals(rhs);
    public static bool operator !=(PathObject lhs, PathObject rhs) => !lhs.Equals(rhs);

    public static PathObject operator -(PathObject lhs, PathObject rhs) => lhs.Relativize(rhs);
    //public void operator -=(PathObject other) => only on C# 14
    
    /// <summary>
    /// Creates a new file on the given path.
    /// </summary>
    /// <param name="filename">The complete name of the file</param>
    /// <returns>The path to the file</returns>
    /// <exception cref="NotSupportedException">If the current path is already an file</exception>
    public PathObject File(string filename) {
        if (!IsDirectory || IsFile) {
            throw new NotSupportedException("Cannot append a file to a file path");
        }

        int dotindex = filename.LastIndexOf('.');
        string extension = dotindex == -1 ? string.Empty : filename[dotindex..];
        string name = dotindex == -1 ? filename : filename[..dotindex];

        return new PathObject() {
            IsFile = true,
            Filename = name,
            Parts = Parts,
            IsAbsolute = IsAbsolute,
            IsDirectory = false,
            Extension = extension
        };
    }

    /// <summary>
    /// Returns the path of the current path. If this is a directory, returns itself. If it's
    /// a file, returns the folder containing this file.
    /// </summary>
    /// <returns>A folder path</returns>
    /// <exception cref="NotSupportedException">Thrown if the path is a file and the filename is empty</exception>
    public PathObject Path()
    {
        if (IsDirectory || !IsFile)
        {
            return this;
        }
        if (string.IsNullOrEmpty(Filename))
        {
            throw new NotSupportedException("Cannot get path from a file without a name");
        }

        return new PathObject
        {
            IsDirectory = true,
            IsFile = false,
            Filename = string.Empty,
            Extension = string.Empty,
            IsAbsolute = IsAbsolute,
            Parts = Parts
        };
    }

    /// <summary>
    /// Subtracts one path from another. Useful to extract a relative path and place on another root.
    /// </summary>
    /// <param name="root">The root to remove from the current path</param>
    /// <returns>The new relative path</returns>
    /// <exception cref="NotSupportedException">Thrown if <see cref="root"/> has incompatible parts with
    /// the current, or it's not a folder</exception>
    public PathObject Relativize(PathObject root) {
        if (root.Parts.Length > Parts.Length) {
            throw new NotSupportedException("Root path cannot contain more parts than fullpath");
        }

        if (root.IsFile || !root.IsDirectory) {
            throw new NotSupportedException("Root path must be a folder");
        }
        for (int i = 0; i < root.Parts.Length; i++) {
            if (Parts[i] != root.Parts[i]) {
                throw new NotSupportedException(
                    $"On of the prefix parts doesn't match! Expected: {Parts[i]} on {nameof(root)}. Got: {root.Parts[i]}");
            }
        }
        return this with { IsAbsolute = false, Parts = Parts[root.Parts.Length..] };
    }

    public XmlSchema? GetSchema() => null!;

    public void ReadXml(XmlReader reader)
    {
        bool isDir = bool.Parse(reader.MoveToAttribute("directory") ? reader.Value : "false");
        string path = reader.MoveToAttribute("path") ? reader.Value : string.Empty;
        reader.Skip();

        PathObject obj = isDir ? path.ToDirectoryPath() : path.ToFilePath();

        Unsafe.AsRef(in this) = obj;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("directory", IsDirectory.ToString());
        writer.WriteAttributeString("path", ToString());
    }

    public bool Equals(PathObject other) {
        if (other.Parts.IsDefault || Parts.IsDefault) return false;
        if (other.Parts.Length != Parts.Length) return false;
        if (other.IsAbsolute != IsAbsolute) return false;
        if (other.IsDirectory != IsDirectory) return false;
        if (other.IsFile != IsFile) return false;
        if (IsFile && (other.Filename != Filename || other.Extension != Extension)) return false;
        return Parts.SequenceEqual(other.Parts);
    }

    public override bool Equals(object? obj) {
        return obj is PathObject other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Parts, IsAbsolute, IsDirectory, IsFile, Filename, Extension);
    }
} 