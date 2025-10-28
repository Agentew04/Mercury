using Mercury.Engine.Common;
using Mercury.Engine.Mips.Runtime.Simple;

namespace Mercury.Engine.Mips.Runtime.OS;

/// <summary>
/// Common interface all operating systems must implement.
/// Specific version for MIPS archtecture.
/// </summary>
public abstract class MipsOperatingSystem : IOperatingSystem {
    
    public WeakReference<Machine?> Machine { get; set; }

    public Architecture CompatibleArchitecture => Architecture.Mips;
    
    public abstract string FriendlyName { get; }
    
    public abstract string Identifier { get; }

    public async Task OnSignalBreak(Monocycle.SignalExceptionEventArgs eventArgs) {
        if (eventArgs.Signal != Monocycle.SignalExceptionEventArgs.SignalType.SystemCall) {
            return;
        }

        uint mask = 0xF_FFFF << 6;
        // this signal is embedded in syscall (normally not used)
        // used when: 'syscall 5'
        uint instructionSignal = (uint)eventArgs.Instruction & mask;
        if (instructionSignal != 0) {
            await OnSyscall(instructionSignal);
        }
        else {
            // this is normally used on mips
            if (Machine.TryGetTarget(out Machine? target))
            {
                uint registerSignal = (uint)target.Registers[MipsGprRegisters.V0];
                await OnSyscall(registerSignal);
            }
        }
    }

    /// <summary>
    /// Function that will be called when a syscall is executed.
    /// </summary>
    /// <param name="code"></param>
    protected abstract ValueTask OnSyscall(uint code);

    public abstract void Dispose();
    
}