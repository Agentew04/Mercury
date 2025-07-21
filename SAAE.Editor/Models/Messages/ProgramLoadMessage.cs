using CommunityToolkit.Mvvm.Messaging.Messages;
using ELFSharp.ELF;
using Machine = SAAE.Engine.Mips.Runtime.Machine;

namespace SAAE.Editor.Models.Messages;

public class ProgramLoadMessage
{
    public required Machine Machine { get; init; }
    
    public required ELF<uint> Elf { get; init; }
}