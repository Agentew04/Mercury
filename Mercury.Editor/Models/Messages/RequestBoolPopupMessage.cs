using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Mercury.Editor.Models.Messages;

public class BoolPopupResult {
    public bool Result { get; init; }
    public bool IsCancelled { get; init; }
}

public class RequestBoolPopupMessage : AsyncRequestMessage<BoolPopupResult> {
    public required string Title { get; init; }
    public bool IsCancellable { get; init; }
}