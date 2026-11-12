using System.Text;
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
        byteSource = new MemoryByteSource(bus, endianness);
        subscriptions.Add(bus.Subscribe<Monocycle,ClockEvent>(this, static async (s,_) => await s.ClockAsync()));
        subscriptions.Add(bus.Subscribe<Monocycle,HaltEvent>(this, static (s,e) => s.Halt(e.ExitCode, publish: false)));
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
    private readonly Endianness endianness = Endianness.LittleEndian;
    private MemoryByteSource byteSource = null!;
    private readonly List<IInstructionExtension> instructionExtensions = [];
    private readonly Memory<byte> instructionBuffer = new byte[4];
    private readonly Memory<byte> memoryBuffer = new byte[4];
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
        byteSource.StartAt(pc);
        IInstruction? instruction = Decode(byteSource);
        if (instruction is null) {
            eventBus.Publish(new UnknownInstructionEvent { // TODO: this event is from mips runtime
                Address = pc,
                InstructionWord = ReadFetchedWord(byteSource, 4)
            });
            Halt(-1);
            return;
        }
        FillInstructionBuffer(byteSource, instruction.Size);
        // execute
        await Execute(instruction);
        
        // next
        Registers.Set(Rv32Gpr.Pc, (int)(pc + (uint)instruction.Size));
    }

    private IInstruction? Decode(IInstructionByteSource source) {
        // check if it's a base ISA instruction
        IInstruction? instruction = Disassembler.Disassemble(source, instructionPool);
        if(instruction is null) {
            // try to check if any extensions recognize this instruction
            uint instructionBinary = ReadFetchedWord(source, 4);
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

    #region Serialization

    public void Serialize(Stream s) {
        using BinaryWriter bw = new(s, Encoding.UTF8, leaveOpen: true);
        
        bw.Write(isHalted);
        bw.Write(ProgramEnd);
        bw.Write(ExitCode);
        
        // registers
        Registers.SerializeBank<Rv32Gpr>(bw);
    }
    
    public void Deserialize(Stream s) {
        using BinaryReader br = new(s, Encoding.UTF8, leaveOpen: true);

        isHalted = br.ReadBoolean();
        ProgramEnd = br.ReadUInt32();
        ExitCode = br.ReadInt32();
        
        // registers
        Registers.DeserializeBank<Rv32Gpr>(br);
    }

    #endregion
}