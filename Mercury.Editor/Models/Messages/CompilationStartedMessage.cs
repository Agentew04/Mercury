using System;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Mercury.Editor.Models.Messages;

public class CompilationStartedMessage(Guid value) : ValueChangedMessage<Guid>(value);