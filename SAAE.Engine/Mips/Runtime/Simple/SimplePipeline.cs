using SAAE.Engine.Common;
using SAAE.Engine.Common.Pipeline;
using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Runtime.Simple.Pipeline;

namespace SAAE.Engine.Mips.Runtime.Simple;

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
        fetchIdBarrier.Advance();
        idExecuteBarrier.Advance();
        executeMemoryBarrier.Advance();
        memoryWriteBackBarrier.Advance();
    }

    public ValueTask ClockAsync() {
        Clock();
        return ValueTask.CompletedTask;
    }

    public bool IsClockingFinished() {
        return (uint)RegisterBank.Get(MipsGprRegisters.Pc) >= DropoffAddress
               || isHalted;
    }

    #endregion

    #region Stages

    private FetchDecodeData DoFetch(ExecuteMemoryData data)
    {
        // int instructionBinary = MipsMachine.DataMemory.ReadWord((ulong)pc);


        return default;
    }

    private DecodeExecuteData DoDecode(FetchDecodeData data) {
        return default;
    }

    private ExecuteMemoryData DoExecute(DecodeExecuteData data) {
        return default;
    }

    private MemoryWriteBackData DoMemory(ExecuteMemoryData data) {
        return default;
    }

    private int DoWriteBack(MemoryWriteBackData data) {
        return default;
    }

    #endregion
}