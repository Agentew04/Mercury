using Mercury.Engine.Common;
using Mercury.Engine.RiscV.RV32I.Instructions;

namespace Mercury.Engine.RiscV.RV32I.Runtime;

public partial class Monocycle {
    private ValueTask<bool> ExecuteTypeU(IInstruction instruction) {
        switch (instruction) {
            case Auipc auipc: {
                int pc = Registers.Get(Rv32Gpr.Pc);
                Registers.Set<Rv32Gpr>(auipc.Rd, pc + (auipc.Immediate << 12));
                break;
            }
            case Lui lui: {
                Registers.Set<Rv32Gpr>(lui.Rd, lui.Immediate << 12);
                break;
            }
            default: {
                return ValueTask.FromResult(false);
            }
        }
        return ValueTask.FromResult(true);
    }
}