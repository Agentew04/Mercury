namespace Mercury.Engine.Mips.Runtime.Events;

using Mercury.Engine.Common;

/// <summary>
/// Published by the CPU when it decoded an instruction but has no
/// implementation for it. Other modules (e.g. the <c>Fpu</c>) may listen,
/// execute the instruction if they recognize it and set <see cref="Handled"/>.
/// </summary>
public sealed class UnhandledInstructionEvent {
    public required IInstruction Instruction { get; init; }
    public uint Word { get; init; }
    public ulong Address { get; init; }

    /// <summary>
    /// Set to <c>true</c> by the module that executed the instruction.
    /// </summary>
    public bool Handled { get; set; }
}

/// <summary>
/// Requests the CPU to take a relative branch. Published by coprocessors (e.g. BC1T/BC1F).
/// </summary>
public readonly struct BranchRequestEvent {
    /// <summary>Offset in instructions, relative to PC+4.</summary>
    public int Offset { get; init; }
}

/// <summary>
/// Published by the FPU whenever its condition flags change.
/// </summary>
public readonly struct FpuFlagsChangedEvent;
