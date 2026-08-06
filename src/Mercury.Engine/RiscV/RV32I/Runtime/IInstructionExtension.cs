using Mercury.Engine.Common;

namespace Mercury.Engine.RiscV.RV32I.Runtime;

public interface IInstructionExtension
{
    public IInstruction? Decode(uint binary);
}