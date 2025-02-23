using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Runtime.Simple;

namespace SAAE.Engine.Mips.Runtime;

/// <summary>
/// Common interface all operating systems must implement.
/// Specific version for MIPS archtecture.
/// </summary>
public abstract class OperatingSystem : IDisposable {

    public Machine Machine { get; set; } = null!;

    public abstract string OperatingSystemName { get; }

    public void OnSignalBreak(Monocycle.SignalExceptionEventArgs eventArgs) {
        if (eventArgs.Signal != Monocycle.SignalExceptionEventArgs.SignalType.SystemCall) {
            throw new InvalidOperationException($"Invalid signal type. Expected {Monocycle.SignalExceptionEventArgs.SignalType.SystemCall}, " +
                                                $"got: {eventArgs.Signal}");
        }

        uint mask = 0xF_FFFF << 6;
        // this signal is embedded in syscall (normally not used)
        // used when: 'syscall 5'
        uint instructionSignal = (uint)eventArgs.Instruction & mask;
        if (instructionSignal != 0) {
            OnSyscall(instructionSignal);
        }
        else {
            // this is normally used on mips
            uint registerSignal = (uint)Machine.Registers[RegisterFile.Register.V0];
            OnSyscall(registerSignal);
        }
    }

    /// <summary>
    /// Function that will be called when a syscall is executed.
    /// </summary>
    /// <param name="code"></param>
    protected abstract void OnSyscall(uint code);

    public abstract void Dispose();
}