namespace SAAE.Editor.Models.Messages;

public class FileDeleteMessage {
    /// <summary>
    /// A reference to the node in the project tree that corresponds to the file that was deleted.
    /// </summary>
    public required ProjectNode ProjectNode { get; init; }
}