using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Runtime.Simple;

namespace SAAE.Engine.Mips.Runtime;

/// <summary>
/// Common interface all operating systems must implement.
/// Specific version for MIPS archtecture.
/// </summary>
public abstract class OperatingSystem {

    public Machine Machine { get; set; } = null!;
    
    public void OnSignalBreak(Monocycle.SignalExceptionEventArgs eventArgs) {
        if (eventArgs.Signal != Monocycle.SignalExceptionEventArgs.SignalType.SystemCall) {
            throw new InvalidOperationException($"Invalid signal type. Expected {Monocycle.SignalExceptionEventArgs.SignalType.SystemCall}, " +
                                                $"got: {eventArgs.Signal}");
        }

        uint mask = 0xF_FFFF << 6;
        uint signal = (uint)eventArgs.Instruction & mask;
        OnSyscall(signal);
    }

    /// <summary>
    /// Function that will be called when a syscall is executed.
    /// </summary>
    /// <param name="code"></param>
    protected abstract void OnSyscall(uint code);
}