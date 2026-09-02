using Mercury.Engine.Common;
using Mercury.Engine.RiscV.RV32I.Instructions;

namespace Mercury.Engine.RiscV.RV32I.Runtime;

public partial class Monocycle {
    private ValueTask<bool> ExecuteTypeS(IInstruction instruction) {

        switch (instruction) {
            case Beq beq: {
                int rs1 = Registers.Get<Rv32Gpr>(beq.Rs1);
                int rs2 = Registers.Get<Rv32Gpr>(beq.Rs2);
                if (rs1 == rs2) {
                    // branch
                    Registers.Set(Rv32Gpr.Pc, (int)((uint)Registers.Get(Rv32Gpr.Pc)+beq.Imm));
                }
                break;
            }
            case Bge bge: {
                int rs1 = Registers.Get<Rv32Gpr>(bge.Rs1);
                int rs2 = Registers.Get<Rv32Gpr>(bge.Rs2);
                if (rs1 >= rs2) {
                    // branch
                    Registers.Set(Rv32Gpr.Pc, (int)((uint)Registers.Get(Rv32Gpr.Pc)+bge.Imm));
                }
                break;
            }
            case Bgeu bgeu: {
                uint rs1 = (uint)Registers.Get<Rv32Gpr>(bgeu.Rs1);
                uint rs2 = (uint)Registers.Get<Rv32Gpr>(bgeu.Rs2);
                if (rs1 >= rs2) {
                    // branch
                    Registers.Set(Rv32Gpr.Pc, (int)((uint)Registers.Get(Rv32Gpr.Pc)+bgeu.Imm));
                }
                break;
            }
            case Blt blt: {
                int rs1 = Registers.Get<Rv32Gpr>(blt.Rs1);
                int rs2 = Registers.Get<Rv32Gpr>(blt.Rs2);
                if (rs1 < rs2) {
                    // branch
                    Registers.Set(Rv32Gpr.Pc, (int)((uint)Registers.Get(Rv32Gpr.Pc)+blt.Imm));
                }
                break;
            }
            case Bltu bltu: {
                uint rs1 = (uint)Registers.Get<Rv32Gpr>(bltu.Rs1);
                uint rs2 = (uint)Registers.Get<Rv32Gpr>(bltu.Rs2);
                if (rs1 < rs2) {
                    // branch
                    Registers.Set(Rv32Gpr.Pc, (int)((uint)Registers.Get(Rv32Gpr.Pc)+bltu.Imm));
                }
                break;
            }
            case Bne bne: {
                int rs1 = Registers.Get<Rv32Gpr>(bne.Rs1);
                int rs2 = Registers.Get<Rv32Gpr>(bne.Rs2);
                if (rs1 != rs2) {
                    // branch
                    Registers.Set(Rv32Gpr.Pc, (int)((uint)Registers.Get(Rv32Gpr.Pc)+bne.Imm));
                }
                break;
            }
            case Jal jal: {
                uint pc = (uint)Registers.Get(Rv32Gpr.Pc); 
                Registers.Set<Rv32Gpr>(jal.Rd, (int)(pc+4));
                
                Registers.Set(Rv32Gpr.Pc, (int)(pc + jal.Imm));
                break;
            }
            default: {
                return ValueTask.FromResult(false);
            }
        }
        
        return ValueTask.FromResult(true);
    }
}