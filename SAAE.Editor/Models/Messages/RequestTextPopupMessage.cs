using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SAAE.Editor.Models.Messages;

public class TextPopupResult {
    public required string Result { get; init; }
    public required bool IsCancelled { get; init; }
}

/// <summary>
/// Message that request text input from the user in a modal.
/// </summary>
public class RequestTextPopupMessage : AsyncRequestMessage<TextPopupResult> {

    /// <summary>
    /// The title of the dialog box. Optional
    /// </summary>
    public string? Title { get; init; } = null;

    /// <summary>
    /// An optional watermark to use in the text field.
    /// </summary>
    public string? Watermark { get; init; } = null;

    /// <summary>
    /// Wether this request is cancellable by the user or not.
    /// </summary>
    public bool IsCancellable { get; init; } = true;
}