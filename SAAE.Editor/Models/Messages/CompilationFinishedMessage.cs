using System;
using CommunityToolkit.Mvvm.Messaging.Messages;
using SAAE.Editor.Models.Compilation;

namespace SAAE.Editor.Models.Messages;

public class CompilationFinishedMessage(CompilationResult value) : ValueChangedMessage<CompilationResult>(value);