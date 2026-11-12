using Mercury.Engine.Common;
using Mercury.Engine.RiscV.RV32I.Instructions;

namespace Mercury.Engine.RiscV.RV32I.Runtime;

public partial class Monocycle {
    private ValueTask<bool> ExecuteTypeI(IInstruction instruction) {
        switch (instruction) {
            case Addi addi: {
                Registers.Set<Rv32Gpr>(addi.Rd, Registers.Get<Rv32Gpr>(addi.Rs1) + addi.Imm);
                break;
            }
            case Andi andi: {
                Registers.Set<Rv32Gpr>(andi.Rd, Registers.Get<Rv32Gpr>(andi.Rs1) & andi.Imm);
                break;
            }
            case Jalr jalr: {
                int pc4 = Registers.Get(Rv32Gpr.Pc) + 4;
                Registers.Set(Rv32Gpr.Pc, (Registers.Get<Rv32Gpr>(jalr.Rs1)+jalr.Imm) & ~1);
                Registers.Set<Rv32Gpr>(jalr.Rd, pc4);
                break;
            }
            case Lb lb: {
                ulong address = (ulong)((uint)Registers.Get<Rv32Gpr>(lb.Rs1) + lb.Imm);
                ReadMemory(address, memoryBuffer[..1]);
                Registers.Set<Rv32Gpr>(lb.Rd, (sbyte)memoryBuffer.Span[0]);
                break;
            }
            case Lbu lbu: {
                ulong address = (ulong)((uint)Registers.Get<Rv32Gpr>(lbu.Rs1) + lbu.Imm);
                ReadMemory(address, memoryBuffer[..1]);
                Registers.Set<Rv32Gpr>(lbu.Rd, memoryBuffer.Span[0]);
                break;
            }
            case Lw lw: {
                ulong address = (ulong)((uint)Registers.Get<Rv32Gpr>(lw.Rs1) + lw.Imm);
                // if((address & 0b11) != 0) {
                //     eventBus.Publish(new UnalignedMemoryAccessEvent {
                //         InstructionWord = FetchedWord,
                //         AccessSize = 4,
                //         InstructionAddress = (ulong)Registers.Get(MipsGprRegisters.Pc),
                //         MemoryAddress = address
                //     });
                //     break;
                // }
                ReadMemory(address, memoryBuffer);
                int value = BytesToInt32(memoryBuffer.Span);
                Registers.Set<Rv32Gpr>(lw.Rd, value);
                break;
            }
            case Ori ori: {
                break;
            }
            case Sb sb: {
                break;
            }
            case Sh sh: {
                break;
            }
            case Slli slli: {
                break;
            }
            case Slti slti: {
                break;
            }
            case Sltiu sltiu: {
                break;
            }
            case Srai srai: {
                break;
            }
            case Srli srli: {
                break;
            }
            case Sw sw: {
                break;
            }
            case Xori xori: {
                break;
            }
            default: {
                return ValueTask.FromResult(false);
            }
        }
        return ValueTask.FromResult(true);
    }
}