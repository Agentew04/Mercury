using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SAAE.Editor.Models.Messages;

public class FileOpenMessage : ValueChangedMessage<ProjectNode> {
    public FileOpenMessage(ProjectNode value) : base(value) {
        
    }
}