using Mercury.Engine.Common;
using Mercury.Engine.Memory;
using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Mips.Runtime.Simple;

/// <summary>
/// A simplified version of the monocycle MIPS processor.
/// Does not simulate every component of the processor.
/// </summary>
public sealed partial class Monocycle : IAsyncClockable, IMipsCpu {
    public Monocycle() {
        RegisterBank.DefineBank<MipsGprRegisters>(MipsRegisterHelper.GetMipsGprRegistersCount());
        RegisterBank.DefineBank<MipsFpuRegisters>(MipsRegisterHelper.GetMipsFpuRegistersCount());
        RegisterBank.DefineBank<MipsFpuControlRegisters>(MipsRegisterHelper.GetMipsFpuControlRegistersCount());
        RegisterBank.DefineBank<MipsSpecialRegisters>(MipsRegisterHelper.GetMipsSpecialRegistersCount());

        RegisterBank.Set(MipsGprRegisters.Sp, 0x7FFF_EFFC);
        RegisterBank.Set(MipsGprRegisters.Fp, 0x0000_0000);
        RegisterBank.Set(MipsGprRegisters.Gp, 0x1000_8000);
        RegisterBank.Set(MipsGprRegisters.Ra, 0x0000_0000);
        RegisterBank.Set(MipsGprRegisters.Pc, 0x0040_0000);
        _ = RegisterBank.GetDirty();
    }

    /// <summary>
    /// Represents the RAM memory
    /// </summary>
    public IMemory Memory { get; set; } = null!;

    public MipsMachine MipsMachine { get; set; } = null!;

    /// <summary>
    /// Structure that holds all the general purpose
    /// registers of the CPU.
    /// </summary>
    public RegisterBank RegisterBank { get; } = new(new MipsRegisterHelper());
    
    public bool[] Flags { get; } = new bool[8];
    
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
        int instructionBinary = MipsMachine.InstructionMemory.ReadWord((ulong)RegisterBank.Get(MipsGprRegisters.Pc));

        // decode
        Instruction? instruction = Disassembler.Disassemble((uint)instructionBinary);
        if(instruction is null) {
            if (SignalException is null) return;
            await SignalException.Invoke(new SignalExceptionEventArgs {
                Signal = SignalExceptionEventArgs.SignalType.InvalidInstruction,
                ProgramCounter = RegisterBank.Get(MipsGprRegisters.Pc),
                Instruction = instructionBinary
            });
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
        if (SignalException is not null) {
            await SignalException.Invoke(new SignalExceptionEventArgs {
                Signal = SignalExceptionEventArgs.SignalType.Halt,
                ProgramCounter = RegisterBank.Get(MipsGprRegisters.Pc),
                Instruction = MipsMachine.InstructionMemory.ReadWord((ulong)RegisterBank.Get(MipsGprRegisters.Pc))
            });
            
        }
    }
    
    public event Func<SignalExceptionEventArgs, Task>? SignalException;

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

}