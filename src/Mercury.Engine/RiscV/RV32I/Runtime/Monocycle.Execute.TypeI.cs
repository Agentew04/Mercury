using Mercury.Engine.Common;

namespace Mercury.Engine.RiscV.RV32I.Runtime;

public partial class Monocycle {
    private ValueTask<bool> ExecuteTypeI(IInstruction instruction) {
        
        return ValueTask.FromResult(false);
    }
}