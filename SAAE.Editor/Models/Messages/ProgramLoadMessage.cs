using CommunityToolkit.Mvvm.Messaging.Messages;
using SAAE.Engine.Mips.Runtime;

namespace SAAE.Editor.Models.Messages;

public class ProgramLoadMessage
{
    public required Machine Machine { get; init; }
}