namespace Mercury.Engine.Common;

public interface IInstructionDisassembler
{
    public IInstruction? Decode(uint instruction, IInstructionPool pool);
}