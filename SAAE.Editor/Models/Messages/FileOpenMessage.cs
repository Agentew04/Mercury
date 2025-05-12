using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SAAE.Editor.Models.Messages;

public class FileOpenMessage(ProjectNode value) : ValueChangedMessage<ProjectNode>(value);