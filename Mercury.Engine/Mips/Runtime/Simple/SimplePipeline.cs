using Mercury.Engine.Common.Pipeline;
using Mercury.Engine.Mips.Runtime;
using Mercury.Engine.Mips.Runtime.Simple.Pipeline;
using Mercury.Engine.Common;
using Mercury.Engine.Memory;
using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Mips.Runtime.Simple;

/// <summary>
/// Simple version of the MIPS CPU but with a standard 5-stage
/// pipeline. It aims to be a cycle accurate simulator
/// </summary>
public class SimplePipeline : IMipsCpu
{
    public SimplePipeline()
    {
        // Initialize the registers
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
        
        // Initialize the pipeline stages
        fetchStage = new PipelineStage<ExecuteMemoryData, FetchDecodeData>(executeMemoryBarrier, fetchIdBarrier, DoFetch);
        idStage = new PipelineStage<FetchDecodeData, DecodeExecuteData>(fetchIdBarrier, idExecuteBarrier, DoDecode);
        executeStage =
            new PipelineStage<DecodeExecuteData, ExecuteMemoryData>(idExecuteBarrier, executeMemoryBarrier, DoExecute);
        memoryStage =
            new PipelineStage<ExecuteMemoryData, MemoryWriteBackData>(executeMemoryBarrier, memoryWriteBackBarrier,
                DoMemory);
        writebackStage = new PipelineStage<MemoryWriteBackData, int>(memoryWriteBackBarrier, null, DoWriteBack);
    }

    #region Components

    /// <summary>
    /// A reference to the machine that this CPU is part of.
    /// </summary>
    public MipsMachine MipsMachine { get; set; } = null!;

    public event Func<SignalExceptionEventArgs, Task>? SignalException;

    /// <summary>
    /// Structure that holds all the registers of the CPU.
    /// </summary>
    public RegisterBank RegisterBank { get; set; } = new(new MipsRegisterHelper());

    public int ExitCode { get; set; }
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

    #endregion

    #region Properties

    /// <summary>
    /// The first invalid address that the program cannot execute.
    /// </summary>
    public uint DropoffAddress { get; set; } = 0;
    
    /// <summary>
    /// Defines if the cpu is halted or not.
    /// </summary>
    private bool isHalted = false;

    private readonly PipelineStage<ExecuteMemoryData,FetchDecodeData> fetchStage; // ex/mem loopback
    private readonly TemporalBarrier<FetchDecodeData> fetchIdBarrier = new();
    private readonly PipelineStage<FetchDecodeData, DecodeExecuteData> idStage;
    private readonly TemporalBarrier<DecodeExecuteData> idExecuteBarrier = new();
    private readonly PipelineStage<DecodeExecuteData, ExecuteMemoryData> executeStage;
    private readonly TemporalBarrier<ExecuteMemoryData> executeMemoryBarrier = new();
    private readonly PipelineStage<ExecuteMemoryData, MemoryWriteBackData> memoryStage;
    private readonly TemporalBarrier<MemoryWriteBackData> memoryWriteBackBarrier = new();
    private readonly PipelineStage<MemoryWriteBackData, int> writebackStage;
    
    #endregion
    
    #region Public Methods

    public void Clock()
    {
        if (isHalted) {
            return;
        }
        
        // consume previous data and produce new data
        fetchStage.Tick();
        idStage.Tick();
        executeStage.Tick();
        memoryStage.Tick();
        writebackStage.Tick();
        
        // commit values produced
        fetchIdBarrier.Commit();
        idExecuteBarrier.Commit();
        executeMemoryBarrier.Commit();
        memoryWriteBackBarrier.Commit();
    }

    public ValueTask ClockAsync() {
        Clock();
        return ValueTask.CompletedTask;
    }

    public bool IsClockingFinished() {
        return (uint)RegisterBank.Get(MipsGprRegisters.Pc) >= (DropoffAddress+5*4)
               || isHalted;
    }

    #endregion

    #region Stages

    private FetchDecodeData DoFetch(ExecuteMemoryData? data)
    {
        uint pc = (uint)RegisterBank.Get(MipsGprRegisters.Pc);
        
        // fetch
        uint instruction = (uint)MipsMachine.InstructionMemory.ReadWord(pc);
        
        // pc+4 or branched
        uint newPc = pc+4;
        if (data?.BranchTaken ?? false) {
            newPc = data.AluResult;
        }
        
        Console.WriteLine($"Fetch: {instruction:X8} @ {pc:X8} -> {newPc:X8}");
        // set pc recursive
        RegisterBank.Set(MipsGprRegisters.Pc, (int)newPc);
        
        return new FetchDecodeData {
            NewPc = newPc,
            Instruction = instruction
        };
    }

    private DecodeExecuteData DoDecode(FetchDecodeData? data) {

        // Decode. If null, nop
        Instruction? inst = Disassembler.Disassemble(data?.Instruction ?? 0);
        if (inst is null) {
            Console.WriteLine("Invalid instruction! Replacing for NOP");
        }
        inst ??= new Nop();
        
        // read registers
        int rs = 0;
        int rt = 0;
        int rd = 0;
        if (inst is TypeRInstruction typeR) {
            rs = typeR.Rs;
            rt = typeR.Rt;
            rd = typeR.Rd;
        }else if (inst is TypeIInstruction typeI) {
            rs = typeI.Rs;
            rd = typeI.Rt; // RT is return in Type I
        }
        
        // sign extend immediate
        int immediate = 0;
        if (inst is TypeIInstruction typeIInst) {
            immediate = typeIInst.Immediate; // sign-extend
        }
        
        int rsValue = RegisterBank.Get<MipsGprRegisters>(rs);
        int rtValue = RegisterBank.Get<MipsGprRegisters>(rt);
        
        Console.WriteLine($"Decode: {inst.GetType().Name} RsValue:{rsValue} RtValue:{rtValue} Rd:{rd} Imm:{immediate}");

        return new DecodeExecuteData() {
            ImmediateExtended = immediate,
            RsValue = rsValue,
            RtValue = rtValue,
            Instruction = inst,
            WriteBackRegister = rd
        };
    }

    private ExecuteMemoryData DoExecute(DecodeExecuteData? data) {
        if (data is null) {
            return new ExecuteMemoryData();
        }

        return default;
    }

    private MemoryWriteBackData DoMemory(ExecuteMemoryData? data) {
        return default;
    }

    private int DoWriteBack(MemoryWriteBackData? data) {
        return default;
    }

    #endregion
}