namespace SAAE.Engine.Mips.Runtime.Simple.PipelineStages;

/// <summary>
/// Enumerator with all the stages of the 5 stage pipeline.
/// </summary>
public enum PipelineStageType
{
    /// <summary>
    /// Stage where the instruction is fetched from the memory.
    /// </summary>
    InstructionFetch,
    /// <summary>
    /// Stage where the opcode is decoded and the registers
    /// are read from the <see cref="SimplePipeline.RegisterFile"/>.
    /// </summary>
    InstructionDecode,
    /// <summary>
    /// Stage where the ALU is used to calculate the result of the
    /// operation or calculate the end address of a jump/branch instruction.
    /// </summary>
    Execute,
    /// <summary>
    /// Stage where the memory is accessed to read or write data.
    /// </summary>
    MemoryAccess,
    /// <summary>
    /// Stage where the result of the operation is written back to the
    /// <see cref="SimplePipeline.RegisterFile"/>.
    /// </summary>
    WriteBack
}
