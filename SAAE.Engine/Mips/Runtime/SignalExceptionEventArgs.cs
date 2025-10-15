namespace SAAE.Engine.Mips.Runtime;

public class SignalExceptionEventArgs {
    public SignalType Signal { get; init; }

    public int ProgramCounter { get; init; }

    public int Instruction { get; init; }

    public enum SignalType {
        Breakpoint,
        SystemCall,
        Trap,
        IntegerOverflow,
        AddressError,
        Halt,
        InvalidInstruction,
        InvalidOperation,
    }
}