using Mercury.Engine.Common;

namespace Mercury.Engine.Mips.Runtime.OS;

/// <summary>
/// Common interface all operating systems must implement.
/// Specific version for MIPS archtecture.
/// </summary>
public abstract class MipsOperatingSystem : IOperatingSystem {

    public MipsMachine MipsMachine { get; set; } = null!;

    public Machine Machine {
        get => MipsMachine;
        set {
            if (value is not MipsMachine m) {
                throw new InvalidOperationException("Tried setting a not mips machine in MipsOperatingSystem.");
            }
            MipsMachine = m;
        }
    }

    public Architecture CompatibleArchitecture => Architecture.Mips;
    
    public abstract string FriendlyName { get; }
    
    public abstract string Identifier { get; }

    public async Task OnSignalBreak(SignalExceptionEventArgs eventArgs) {
        if (eventArgs.Signal != SignalExceptionEventArgs.SignalType.SystemCall) {
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
            uint registerSignal = (uint)Machine.Registers.Get(MipsGprRegisters.V0);
            await OnSyscall(registerSignal);
        }
    }

    /// <summary>
    /// Function that will be called when a syscall is executed.
    /// </summary>
    /// <param name="code"></param>
    protected abstract ValueTask OnSyscall(uint code);

    public abstract void Dispose();
    
}