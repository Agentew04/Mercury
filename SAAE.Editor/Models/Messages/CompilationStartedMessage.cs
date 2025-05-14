using System;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SAAE.Editor.Models.Messages;

public class CompilationStartedMessage(Guid value) : ValueChangedMessage<Guid>(value);