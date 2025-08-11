using SAAE.Engine.Common;
using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Instructions;

namespace SAAE.Engine.Mips.Runtime.Simple;

/// <summary>
/// A simplified version of the monocycle MIPS processor.
/// Does not simulate every component of the processor.
/// </summary>
public sealed partial class Monocycle : IAsyncClockable {
    public Monocycle() {
        RegisterBank.DefineBank<MipsGprRegisters>(RegisterHelper.GetMipsGprRegistersCount());
        RegisterBank.DefineBank<MipsFpuRegisters>(RegisterHelper.GetMipsFpuRegistersCount());
        RegisterBank.DefineBank<MipsFpuControlRegisters>(RegisterHelper.GetMipsFpuControlRegistersCount());
        RegisterBank.DefineBank<MipsSpecialRegisters>(RegisterHelper.GetMipsSpecialRegistersCount());

        RegisterBank.Set(MipsGprRegisters.Sp, 0x7FFF_EFFC);
        RegisterBank.Set(MipsGprRegisters.Fp, 0x0000_0000);
        RegisterBank.Set(MipsGprRegisters.Gp, 0x1000_8000);
        RegisterBank.Set(MipsGprRegisters.Ra, 0x0000_0000);
        RegisterBank.Set(MipsGprRegisters.Pc, 0x0040_0000);
    }

    /// <summary>
    /// Represents the RAM memory
    /// </summary>
    public IMemory Memory { get; set; } = null!;

    public Machine Machine { get; set; } = null!;

    /// <summary>
    /// Structure that holds all the general purpose
    /// registers of the CPU.
    /// </summary>
    public RegisterBank RegisterBank { get; private set; } = new();
    
    public bool[] Flags { get; private set; } = new bool[8];

    /// <summary>
    /// Class responsible to interpret binary instructions
    /// and return the corresponding class.
    /// </summary>
    private readonly InstructionFactory instructionFactory = new();

    /// <inheritdoc cref="instructionFactory"/>
    public InstructionFactory InstructionFactory => instructionFactory;

    public bool UseBranchDelaySlot { get; set; }
    
    public uint DropoffAddress { get; set; }

    private bool isExecutingBranch;
    private bool isNextCycleBranch;
    private uint branchAddress;
    
    private bool isHalted;

    /// <summary>
    /// Gets the exit code of the program.
    /// </summary>
    public int ExitCode { get; private set; }

    public async ValueTask ClockAsync()
    {
        if (isHalted) {
            return;
        }
        // read instruction from PC
        int instructionBinary = Memory.ReadWord((ulong)RegisterBank.Get(MipsGprRegisters.Pc));

        // decode
        Instruction instruction;
        try {
            instruction = instructionFactory.Disassemble((uint)instructionBinary);
        }
        catch (Exception) {
            if (OnSignalException is not null) {
                await OnSignalException.Invoke(new SignalExceptionEventArgs {
                    Signal = SignalExceptionEventArgs.SignalType.InvalidInstruction,
                    ProgramCounter = RegisterBank.Get(MipsGprRegisters.Pc),
                    Instruction = instructionBinary
                });
            }
            await Halt(-1);
            return;
        }

        int pcBefore = RegisterBank.Get(MipsGprRegisters.Pc);
        await Execute(instruction);

        if (isExecutingBranch && isNextCycleBranch)
        {
            // estamos no proximo ciclo, faz o branch
            isExecutingBranch = false;
            isNextCycleBranch = false;

            RegisterBank.Set(MipsGprRegisters.Pc, (int)branchAddress);
        }else if (isExecutingBranch && !isNextCycleBranch)
        {
            // estamos no cliclo do branch. 
            isNextCycleBranch = true;
            // pc+4
            RegisterBank.Set(MipsGprRegisters.Pc, pcBefore + 4);
        }
        else
        {
            // instrucao sem branch
            RegisterBank[MipsGprRegisters.Pc] += 4;
        }
    }

    /// <summary>
    /// Stops all execution of this cpu immediately.
    /// The system cannot be resumed after this.
    /// </summary>
    public async ValueTask Halt(int code = 0) {
        isHalted = true;
        ExitCode = code;
        // tah certo invocar aqui? se for no meio do ciclo
        // os registradores nao estariam certo(branch)
        // mas tbm, soh da halt uma syscall, entao branch nunca executa esse sinal
        if (OnSignalException is not null) {
            await OnSignalException.Invoke(new SignalExceptionEventArgs {
                Signal = SignalExceptionEventArgs.SignalType.Halt,
                ProgramCounter = RegisterBank.Get(MipsGprRegisters.Pc),
                Instruction = Memory.ReadWord((ulong)RegisterBank.Get(MipsGprRegisters.Pc))
            });
            
        }
    }
    
    public event Func<SignalExceptionEventArgs, Task>? OnSignalException;

    private ValueTask Execute(Instruction instruction) {
        switch (instruction) {
            case TypeRInstruction r:
                return ExecuteTypeR(r);
            case TypeIInstruction i:
                return ExecuteTypeI(i);
            case TypeJInstruction j:
                ExecuteTypeJ(j);
                break;
            case TypeFInstruction f:
                return ExecuteTypeF(f);
        }
        return ValueTask.CompletedTask;
    }

    private static bool IsOverflowed(int a, int b, int result) {
        return (a > 0 && b > 0 && result < 0) || (a < 0 && b < 0 && result > 0);
    }

    private void BranchTo(int immediate) {
        isExecutingBranch = true;
        branchAddress = (uint)(RegisterBank[MipsGprRegisters.Pc] + 4 + (immediate << 2));
    }
    private void Link(MipsGprRegisters register = MipsGprRegisters.Ra) {
        RegisterBank[register] = RegisterBank[MipsGprRegisters.Pc] + (UseBranchDelaySlot ? 8 : 4);
    }

    private static int ZeroExtend(short value) {
        return (ushort)value;
    }

    public bool IsClockingFinished() {
        return RegisterBank[MipsGprRegisters.Pc] >= DropoffAddress
            || isHalted;
    }

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
}
