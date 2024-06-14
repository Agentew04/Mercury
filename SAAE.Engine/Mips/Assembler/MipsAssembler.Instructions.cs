using SAAE.Engine.Mips.Instructions;

namespace SAAE.Engine.Mips.Assembler; 
public partial class MipsAssembler {

    private void RegisterTypeR() {
        supportedInstructions.AddRange([
            new Add(),
            new Addu(),
            new Mul(),
            new Slt(),
            new Sltu(),
            new Sub(),
            new Subu(),
            new And(),
            new Nor(),
            new Or(),
            new Xor(),
            new Sll(),
            new Sllv(),
            new Sra(),
            new Srav(),
            new Srl(),
            new Srlv(),
            new Div(),
            new Divu(),
            new Madd(),
            new Maddu(),
            new Msub(),
            new Msubu(),
            new Mult(),
            new Multu(),
            new Clo(),
            new Clz(),
            new Mfhi(),
            new Mflo(),
            new Mthi(),
            new Mtlo(),
            new Movz(),
            new Movn(),
            new Break(),
            new Syscall(),
            new Teq()
        ]);
    }

    private void RegisterTypeI() {
        supportedInstructions.AddRange([
            new Addi(),
            new Addiu(),
            new Slti(),
            new Sltiu(),
            new Andi(),
            new Ori(),
            new Xori(),
            new Lb(),
            new Lbu(),
            new Lh(),
            new Lhu(),
            new Lui(),
            new Lw(),
            new Sb(),
            new Sh(),
            new Sw(),
            new Beq(),
            new Bgez(),
            new Bgtz(),
            new Blez(),
            new Bltz(),
            new Bne(),
            new Jalr(),
            new Jr(),
            new Teqi()
        ]);
    }

    private void RegisterTypeJ() {
        supportedInstructions.AddRange([
            new J(),
            new Jal()
        ]);
    }
}
