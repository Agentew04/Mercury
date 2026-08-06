using Mercury.Engine.Common;
using Mercury.Engine.Common.Events;
using Mercury.Engine.Memory;
using Mercury.Engine.Mips.Runtime.Events;
using Mercury.Engine.RiscV.RV32I.Instructions;

namespace Mercury.Engine.RiscV.RV32I.Runtime;

public sealed partial class Monocycle : ICpuModule, IDisposable {

    public Monocycle() {
        Registers.DefineGroup<Rv32Gpr, RiscVRegisterHelper>();
    }
    
    #region Event Bus

    private EventBus eventBus = null!;
    private readonly List<IDisposable> subscriptions = [];

    public void SubscribeToEvents(EventBus bus) {
        eventBus = bus;
        subscriptions.Add(bus.Subscribe<ClockEvent>(async _ => await ClockAsync()));
        subscriptions.Add(bus.Subscribe<HaltEvent>(e => Halt(e.ExitCode, publish: false)));
    }

    public void UnsubscribeFromEvents() {
        foreach (IDisposable disposable in subscriptions) {
            disposable.Dispose();
        }

        subscriptions.Clear();
    }

    #endregion

    #region State

    private bool isHalted;
    private Endianess endianess = Endianess.LittleEndian;
    private readonly List<IInstructionExtension> instructionExtensions = [];
    private readonly Memory<byte> instructionBuffer = new byte[4];
    private readonly InstructionPool instructionPool = new();

    #endregion


    public uint ProgramEnd { get; set; }
    public int ExitCode { get; private set; }
    public RegisterCollection Registers { get; } = new(new RiscVRegisterHelper());
    
    
    public async Task ClockAsync() {
        if (isHalted) {
            return;
        }
    
        // read instruction from PC
        ulong pc = (ulong)Registers.Get(Rv32Gpr.Pc);
        ReadMemory(pc,instructionBuffer);
        uint instructionBinary = (uint)BytesToInt32(instructionBuffer.Span);
        IInstruction? instruction = Decode(instructionBinary);
        if (instruction is null) {
            eventBus.Publish(new UnknownInstructionEvent { // TODO: this event is from mips runtime
                Address = pc,
                InstructionWord = instructionBinary
            });
            Halt(-1);
            return;
        }
        // execute
        await Execute(instruction);
        
        // next
        Registers.Set(Rv32Gpr.Pc, (int)(pc + 4));
    }

    private IInstruction? Decode(uint instructionBinary) {
        // check if it's a base ISA instruction
        IInstruction? instruction = Disassembler.Disassemble(instructionBinary, instructionPool);
        if(instruction is null) {
            // try to check if any extensions recognize this instruction
            foreach (var extension in instructionExtensions) {
                instruction = extension.Decode(instructionBinary);
                if (instruction is not null) {
                    break;
                }
            }
        }
        return instruction;
    }

    private async ValueTask Execute(IInstruction instruction) {
        if (await ExecuteTypeI(instruction)) {
            return;
        }

        if (await ExecuteTypeR(instruction)) {
            return;
        }

        if (await ExecuteTypeS(instruction)) {
            return;
        }

        if (await ExecuteTypeU(instruction)) {
            return;
        }
        
        eventBus.Publish(new UntreatedInstructionEvent {
            Address = (ulong)Registers.Get(Rv32Gpr.Pc),
            Word = Convert.ToUInt32(instructionBuffer),
            Description = instruction.ToString()
        });
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
                Address = (ulong)Registers.Get(Rv32Gpr.Pc)
            });
        }
    }
    
    public void Dispose() {
        UnsubscribeFromEvents();
    }
}