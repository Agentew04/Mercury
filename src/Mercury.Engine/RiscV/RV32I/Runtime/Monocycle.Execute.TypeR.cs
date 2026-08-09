using Mercury.Engine.Common;
using Mercury.Engine.RiscV.Events;
using Mercury.Engine.RiscV.RV32I.Instructions;

namespace Mercury.Engine.RiscV.RV32I.Runtime;

public partial class Monocycle {
    private ValueTask<bool> ExecuteTypeR(IInstruction instruction) {

        switch (instruction) {
            case Add add: {
                int rs1 = Registers.Get<Rv32Gpr>(add.Rs1);
                int rs2 = Registers.Get<Rv32Gpr>(add.Rs2);
                Registers.Set<Rv32Gpr>(add.Rd, rs1 + rs2);
                break;
            }
            case And and: {
                int rs1 = Registers.Get<Rv32Gpr>(and.Rs1);
                int rs2 = Registers.Get<Rv32Gpr>(and.Rs2);
                Registers.Set<Rv32Gpr>(and.Rd, rs1 & rs2);
                break;
            }
            case Ebreak: {
                eventBus.Publish(new BreakpointHit());
                break;
            }
            case Ecall: {
                eventBus.Publish(new EnvironmentCall());
                break;
            }
            case Or or: {
                int rs1 = Registers.Get<Rv32Gpr>(or.Rs1);
                int rs2 = Registers.Get<Rv32Gpr>(or.Rs2);
                Registers.Set<Rv32Gpr>(or.Rd, rs1 | rs2);
                break;
            }
            case Sll sll: {
                int rs1 = Registers.Get<Rv32Gpr>(sll.Rs1);
                int rs2 = Registers.Get<Rv32Gpr>(sll.Rs2) & 0b11111;
                Registers.Set<Rv32Gpr>(sll.Rd, rs1 << rs2);
                break;
            }
            case Slt slt: {
                int rs1 = Registers.Get<Rv32Gpr>(slt.Rs1);
                int rs2 = Registers.Get<Rv32Gpr>(slt.Rs2);
                Registers.Set<Rv32Gpr>(slt.Rd, rs1 < rs2 ? 1 : 0);
                break;
            }
            case Sltu sltu: {
                uint rs1 = (uint)Registers.Get<Rv32Gpr>(sltu.Rs1);
                uint rs2 = (uint)Registers.Get<Rv32Gpr>(sltu.Rs2);
                Registers.Set<Rv32Gpr>(sltu.Rd, rs1 < rs2 ? 1 : 0);
                break;
            }
            case Sra sra: {
                int rs1 = Registers.Get<Rv32Gpr>(sra.Rs1);
                int rs2 = Registers.Get<Rv32Gpr>(sra.Rs2) & 0b11111;
                Registers.Set<Rv32Gpr>(sra.Rd, rs1 >> rs2); // >> is arithmetic shift (fills with sign)
                break;
            }
            case Srl srl: {
                int rs1 = Registers.Get<Rv32Gpr>(srl.Rs1);
                int rs2 = Registers.Get<Rv32Gpr>(srl.Rs2) & 0b11111;
                Registers.Set<Rv32Gpr>(srl.Rd, rs1 >>> rs2); // >>> is logical shift (fills with 0s)
                break;
            }
            case Sub sub: {
                long rs1 = Registers.Get<Rv32Gpr>(sub.Rs1);
                long rs2 = Registers.Get<Rv32Gpr>(sub.Rs2);
                Registers.Set<Rv32Gpr>(sub.Rd, (int)(rs1-rs2));
                break;
            }
            case Xor xor: {
                int rs1 = Registers.Get<Rv32Gpr>(xor.Rs1);
                int rs2 = Registers.Get<Rv32Gpr>(xor.Rs2);
                Registers.Set<Rv32Gpr>(xor.Rd, rs1 ^ rs2);
                break;
            }
            default: {
                return ValueTask.FromResult(false);
            }
        }
        
        return ValueTask.FromResult(true);
    }
}