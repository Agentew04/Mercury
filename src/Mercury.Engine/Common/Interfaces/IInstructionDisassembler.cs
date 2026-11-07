namespace Mercury.Engine.Common;

/// <summary>
/// Interface defining methods that a source-generated disasassebler must implement.
/// </summary>
public interface IInstructionDisassembler {
    
    /// <summary>
    /// Decodes one instruction from the given source.
    /// </summary>
    /// <param name="source">The source to read the bytes from</param>
    /// <param name="pool">The instruction pool object to retrieve instruction instances from</param>
    /// <returns>The decoded instruction or null if no instruction did match</returns>
    public IInstruction? Decode(IInstructionByteSource source, IInstructionPool pool);
}