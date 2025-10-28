using Mercury.Editor.Extensions;

namespace Mercury.Editor.Models.Messages;

public class FileMoveMessage {

    public required PathObject OldPath { get; set; }

    public required PathObject NewPath { get; set; }
}