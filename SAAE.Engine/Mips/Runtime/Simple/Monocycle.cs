using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Instructions;

namespace SAAE.Engine.Mips.Runtime.Simple;

/// <summary>
/// A simplified version of the monocycle MIPS processor.
/// Does not simulate every component of the processor.
/// </summary>
public sealed partial class Monocycle : IClockable {
    public Monocycle() {
        RegisterFile[RegisterFile.Register.Sp] = 0x7FFF_EFFC; // o 'E' aparece no MARS
        RegisterFile[RegisterFile.Register.Fp] = 0x0000_0000;
        RegisterFile[RegisterFile.Register.Gp] = 0x1000_8000;
        RegisterFile[RegisterFile.Register.Ra] = 0x0000_0000;
        RegisterFile[RegisterFile.Register.Pc] = 0x0040_0000;
    }

    /// <summary>
    /// Represents the RAM memory
    /// </summary>
    public VirtualMemory Memory { get; set; } = null!;

    /// <summary>
    /// Structure that holds all the general purpose
    /// registers of the CPU.
    /// </summary>
    public RegisterFile RegisterFile { get; private set; } = new();

    /// <summary>
    /// Class responsible to interpret binary instructions
    /// and return the corresponding class.
    /// </summary>
    private readonly InstructionFactory instructionFactory = new();

    public bool UseBranchDelaySlot { get; set; } = false;
    
    public uint DropoffAddress { get; set; } = 0;

    private uint nextBranchAddress = 0;
    
    private bool isHalted = false;

    /// <summary>
    /// Gets the exit code of the program.
    /// </summary>
    public int ExitCode { get; private set; } = 0;

    public void Clock()
    {
        if (isHalted) {
            return;
        }
        // read instruction from PC
        int instructionBinary = Memory.ReadWord((ulong)RegisterFile[RegisterFile.Register.Pc]);
        Console.WriteLine($"Decoding instruction 0x{instructionBinary:X8} @ 0x{RegisterFile[RegisterFile.Register.Pc]:X8}");

        // decode
        Instruction instruction;
        try {
            instruction = instructionFactory.Disassemble((uint)instructionBinary);
        }
        catch (Exception) {
            Console.WriteLine("Invalid instruction");
            return;
        }

        Console.WriteLine($"Executing: {instruction.GetType().Name} @ 0x{RegisterFile[RegisterFile.Register.Pc]:X8} (0x{instructionBinary:X8})");
        int pcBefore = RegisterFile[RegisterFile.Register.Pc];
        Execute(instruction);

        // estamos num ciclo do branch delay slot
        if (nextBranchAddress != 0) {
            // realiza o delayed branch
            RegisterFile[RegisterFile.Register.Pc] = (int)nextBranchAddress;
            nextBranchAddress = 0;
            return;
        }
        
        // update PC
        if (pcBefore == RegisterFile[RegisterFile.Register.Pc]) {
            RegisterFile[RegisterFile.Register.Pc] += 4;
        }else if (pcBefore + 8 == RegisterFile[RegisterFile.Register.Pc] 
                  && UseBranchDelaySlot) {
            // se branch delay slot esta ligado, defer update
            nextBranchAddress = (uint)RegisterFile[RegisterFile.Register.Pc];
            RegisterFile[RegisterFile.Register.Pc] = pcBefore + 4;
        }
    }

    /// <summary>
    /// Stops all execution of this cpu immediately.
    /// The system cannot be resumed after this.
    /// </summary>
    public void Halt(int code = 0) {
        isHalted = true;
        ExitCode = code;
        // tah certo invocar aqui? se for no meio do ciclo
        // os registradores nao estariam certo(branch)
        // mas tbm, soh da halt uma syscall, entao branch nunca executa esse sinal
        OnSignalException?.Invoke(this, new SignalExceptionEventArgs {
            Signal = SignalExceptionEventArgs.SignalType.Halt,
            ProgramCounter = RegisterFile[RegisterFile.Register.Pc],
            Instruction = Memory.ReadWord((ulong)RegisterFile[RegisterFile.Register.Pc])
        });
    }
    
    public event EventHandler<SignalExceptionEventArgs>? OnSignalException = null;

    private void Execute(Instruction instruction) {
        if(instruction is TypeRInstruction r) {
            ExecuteTypeR(r);
        }else if(instruction is TypeIInstruction i) {
            ExecuteTypeI(i);
        }else if(instruction is TypeJInstruction j) {
            ExecuteTypeJ(j);
        }
    }

    private static bool IsOverflowed(int a, int b, int result) {
        return (a > 0 && b > 0 && result < 0) || (a < 0 && b < 0 && result > 0);
    }

    private void BranchTo(int immediate) {
        RegisterFile[RegisterFile.Register.Pc] += 4 + immediate << 2;
    }
    private void Link(RegisterFile.Register register = RegisterFile.Register.Ra) {
        RegisterFile[register] = RegisterFile[RegisterFile.Register.Pc];
    }

    private static int ZeroExtend(short value) {
        return (ushort)value;
    }

    public bool IsClockingFinished() {
        return RegisterFile[RegisterFile.Register.Pc] >= DropoffAddress
            || isHalted;
    }

    public class SignalExceptionEventArgs {
        public SignalType Signal { get; set; }

        public int ProgramCounter { get; set; }

        public int Instruction { get; set; }

        public enum SignalType {
            Breakpoint,
            SystemCall,
            Trap,
            IntegerOverflow,
            AddressError,
            Halt
        }
    }
}
