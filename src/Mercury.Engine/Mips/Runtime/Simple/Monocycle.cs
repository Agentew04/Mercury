using System.Buffers.Binary;
using System.Threading.Tasks.Sources;
using Mercury.Engine.Common;
using Mercury.Engine.Common.Events;
using Mercury.Engine.Memory;
using Mercury.Engine.Mips.Instructions;
using Mercury.Engine.Mips.Runtime.Events;

namespace Mercury.Engine.Mips.Runtime.Simple;

/// <summary>
/// A simplified version of the monocycle MIPS processor.
/// Does not simulate every component of the processor.
/// </summary>
public sealed partial class Monocycle : ICpuModule, IDisposable {
    
    public Monocycle() {
        Registers.DefineGroup<MipsGprRegisters,MipsRegisterHelper>();
        Registers.DefineGroup<MipsFpuRegisters,MipsRegisterHelper>();
        Registers.DefineGroup<MipsFpuControlRegisters,MipsRegisterHelper>();
        Registers.DefineGroup<MipsSpecialRegisters,MipsRegisterHelper>();

        Registers.Set(MipsGprRegisters.Sp, 0x7FFF_EFFC);
        Registers.Set(MipsGprRegisters.Fp, 0x0000_0000);
        Registers.Set(MipsGprRegisters.Gp, 0x1000_8000);
        Registers.Set(MipsGprRegisters.Ra, 0x0000_0000);
        Registers.Set(MipsGprRegisters.Pc, 0x0040_0000);
        _ = Registers.GetDirty(out _);
    }

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
    private EventBus eventBus = null!;
    private readonly Memory<byte> instructionBuffer = new byte[4];
    private readonly InstructionPool pool = new();
    private readonly List<IDisposable> subscriptions = [];
    public Endianess Endianess { get; set; }
    
    public void SubscribeToEvents(EventBus bus) {
        this.eventBus = bus;
        subscriptions.Add(bus.Subscribe<ClockEvent>(async _ => await ClockAsync()));
        subscriptions.Add(bus.Subscribe<HaltEvent>(e => Halt(e.ExitCode, publish: false)));
    }

    public void UnsubscribeFromEvents() {
        foreach(IDisposable disposable in subscriptions) {
            disposable.Dispose();
        }
        subscriptions.Clear();
    }

    /// <summary>
    /// Gets the exit code of the program.
    /// </summary>
    public int ExitCode { get; private set; }

    public async Task ClockAsync() {
        if (isHalted) {
            return;
        }
        // read instruction from PC
        ulong pc = (ulong)Registers.Get(MipsGprRegisters.Pc);
        ReadMemory(pc, instructionBuffer);
        uint instructionBinary = (uint)BytesToInt32(instructionBuffer.Span);

        // decode
        IInstruction? instruction = Disassembler.Disassemble(instructionBinary, pool);
        if(instruction is null) {
            eventBus.Publish(new UnknownInstructionEvent {
                Address = pc,
                InstructionWord = instructionBinary
            });
            Halt(-1);
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
    public void Halt(int code = 0, bool publish = true) {
        isHalted = true;
        ExitCode = code;
        // tah certo invocar aqui? se for no meio do ciclo
        // os registradores nao estariam certo(branch)
        // mas tbm, soh da halt uma syscall, entao branch nunca executa esse sinal
        if (publish) {
            eventBus.Publish(new HaltEvent {
                ExitCode = code,
                Address = (ulong)Registers.Get(MipsGprRegisters.Pc)
            });
        }
    }
    
    public event Func<SignalExceptionEventArgs, Task>? SignalException;

    public event Action? OnFlagUpdate; // hackzinho. substituir quando usar sistema de mensagens

    public bool IsClockingFinished() {
        return Registers.Get(MipsGprRegisters.Pc) >= ProgramEnd
            || isHalted;
    }
    
    public void Dispose() {
        UnsubscribeFromEvents();
    }
}
