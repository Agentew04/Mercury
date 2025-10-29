using System;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Mercury.Editor.Models.Compilation;

namespace Mercury.Editor.Models.Messages;

public class CompilationFinishedMessage(CompilationResult value) : ValueChangedMessage<CompilationResult>(value);