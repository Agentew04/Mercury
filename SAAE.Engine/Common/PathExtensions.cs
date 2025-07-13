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

public readonly struct PathObject : IXmlSerializable{
    
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
        if (other is null) {
            return false;
        }

        PathObject o = other.Value;

        if (o.IsAbsolute != IsAbsolute) return false;
        if (o.IsDirectory != IsDirectory) return false;
        if (o.IsFile != IsFile) return false;
        if (IsFile && (o.Filename != Filename || o.Extension != Extension)) return false;
        return Parts.SequenceEqual(o.Parts);
    }

    public PathObject Folder(string newPart) => Folders(newPart);
    
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

    public static PathObject operator +(PathObject lhs, PathObject rhs) => lhs.Append(rhs);
    public static bool operator ==(PathObject lhs, PathObject rhs) => lhs.Equals(rhs);
    public static bool operator !=(PathObject lhs, PathObject rhs) => !lhs.Equals(rhs);
    
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
} 