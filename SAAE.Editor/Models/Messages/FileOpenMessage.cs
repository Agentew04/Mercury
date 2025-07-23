using SAAE.Editor.Extensions;

namespace SAAE.Editor.Models.Messages;

public class FileOpenMessage
{
    /// <summary>
    /// A reference to the node in the project tree that corresponds to the file to be opened.
    /// </summary>
    public ProjectNode? ProjectNode { get; set; }
    
    /// <summary>
    /// The absolute path to the file to be opened.
    /// </summary>
    public PathObject? Path { get; set; }
    
    /// <summary>
    /// The line number to navigate to when the file is opened.
    /// </summary> 
    public int? LineNumber { get; set; }
    
    /// <summary>
    /// The column number to navigate to when the file is opened.
    /// </summary>
    public int? ColumnNumber { get; set; }
}