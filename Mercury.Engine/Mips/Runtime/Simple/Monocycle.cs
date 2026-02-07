using Mercury.Engine.Common;
using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Mips.Runtime.Simple;

/// <summary>
/// A simplified version of the monocycle MIPS processor.
/// Does not simulate every component of the processor.
/// </summary>
public sealed partial class Monocycle : IMipsCpu {
    
    public Monocycle() {
        Registers.DefineGroup<MipsGprRegisters>(MipsRegisterHelper.GetMipsGprRegistersCount());
        Registers.DefineGroup<MipsFpuRegisters>(MipsRegisterHelper.GetMipsFpuRegistersCount());
        Registers.DefineGroup<MipsFpuControlRegisters>(MipsRegisterHelper.GetMipsFpuControlRegistersCount());
        Registers.DefineGroup<MipsSpecialRegisters>(MipsRegisterHelper.GetMipsSpecialRegistersCount());

        Registers.Set(MipsGprRegisters.Sp, 0x7FFF_EFFC);
        Registers.Set(MipsGprRegisters.Fp, 0x0000_0000);
        Registers.Set(MipsGprRegisters.Gp, 0x1000_8000);
        Registers.Set(MipsGprRegisters.Ra, 0x0000_0000);
        Registers.Set(MipsGprRegisters.Pc, 0x0040_0000);
        _ = Registers.GetDirty(out _);
    }

    public MipsMachine MipsMachine { get; set; } = null!;

    /// <summary>
    /// Structure that holds all the general purpose
    /// registers of the CPU.
    /// </summary>
    public RegisterCollection Registers { get; } = new(new MipsRegisterHelper());
    
    public bool[] Flags { get; } = new bool[8];
    
    public bool UseBranchDelaySlot { get; set; }
    
    public uint ProgramEnd { get; set; }

    private bool isExecutingBranch;
    private bool isNextCycleBranch;
    private uint branchAddress;
    
    private bool isHalted;

    private readonly InstructionPool pool = new();

    /// <summary>
    /// Gets the exit code of the program.
    /// </summary>
    public int ExitCode { get; private set; }

    public async ValueTask ClockAsync() {
        if (isHalted) {
            return;
        }
        // read instruction from PC
        int instructionBinary = MipsMachine.Memory.ReadWord(
            (ulong)Registers.Get(MipsGprRegisters.Pc));

        // decode
        IInstruction? instruction = Disassembler.Disassemble((uint)instructionBinary, pool);
        if(instruction is null) {
            if (SignalException is null) return;
            await SignalException.Invoke(new SignalExceptionEventArgs {
                Signal = SignalExceptionEventArgs.SignalType.InvalidInstruction,
                ProgramCounter = Registers.Get(MipsGprRegisters.Pc),
                Instruction = instructionBinary
            });
            await Halt(-1);
            return;
        }

        // execute
        int pcBefore = Registers.Get(MipsGprRegisters.Pc);
        await Execute(instruction);

        if (isExecutingBranch && isNextCycleBranch) {
            // estamos no proximo ciclo, faz o branch
            isExecutingBranch = false;
            isNextCycleBranch = false;

            Registers.Set(MipsGprRegisters.Pc, (int)branchAddress);
        }else if (isExecutingBranch && !isNextCycleBranch) {
            // estamos no cliclo do branch. 
            isNextCycleBranch = true;
            // pc+4
            Registers.Set(MipsGprRegisters.Pc, pcBefore + 4);
        }else {
            // instrucao sem branch
            Registers.Set(MipsGprRegisters.Pc, Registers.Get(MipsGprRegisters.Pc) + 4);
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
                ProgramCounter = Registers.Get(MipsGprRegisters.Pc),
                Instruction = MipsMachine.Memory.ReadWord((ulong)Registers.Get(MipsGprRegisters.Pc))
            });
            
        }
    }
    
    public event Func<SignalExceptionEventArgs, Task>? SignalException;

    public event Action? OnFlagUpdate; // hackzinho. substituir quando usar sistema de mensagens

    private static bool IsOverflowed(int a, int b, int result) {
        return (a > 0 && b > 0 && result < 0) || (a < 0 && b < 0 && result > 0);
    }

    private void BranchTo(int immediate) {
        isExecutingBranch = true;
        branchAddress = (uint)(Registers.Get(MipsGprRegisters.Pc) + 4 + (immediate << 2));
    }
    private void Link(MipsGprRegisters register = MipsGprRegisters.Ra) {
        Registers.Set(register, Registers.Get(MipsGprRegisters.Pc) + (UseBranchDelaySlot ? 8 : 4));
    }

    private static int ZeroExtend(short value) {
        return (ushort)value;
    }

    public bool IsClockingFinished() {
        return Registers.Get(MipsGprRegisters.Pc) >= ProgramEnd
            || isHalted;
    }

}
