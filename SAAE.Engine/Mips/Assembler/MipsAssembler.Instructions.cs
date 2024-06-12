using SAAE.Engine.Mips.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Assembler; 
public partial class MipsAssembler {

    private void RegisterTypeR() {
        supportedInstructions.Add(new Add());
        supportedInstructions.Add(new Addu());
        supportedInstructions.Add(new Mul());
        supportedInstructions.Add(new Slt());
        supportedInstructions.Add(new Sltu());
        supportedInstructions.Add(new Sub());
        supportedInstructions.Add(new Subu());
        supportedInstructions.Add(new And());
        supportedInstructions.Add(new Nor());
        supportedInstructions.Add(new Or());
        supportedInstructions.Add(new Xor());
        supportedInstructions.Add(new Sll());
        supportedInstructions.Add(new Sllv());
        supportedInstructions.Add(new Sra());
        supportedInstructions.Add(new Srav());
        supportedInstructions.Add(new Srl());
        supportedInstructions.Add(new Srlv());

        supportedInstructions.Add(new Div());
        supportedInstructions.Add(new Divu());
        supportedInstructions.Add(new Madd());
        supportedInstructions.Add(new Maddu());
        supportedInstructions.Add(new Msub());
        supportedInstructions.Add(new Msubu());
        supportedInstructions.Add(new Mult());
        supportedInstructions.Add(new Multu());

        supportedInstructions.Add(new Clo());
        supportedInstructions.Add(new Clz());
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
            new Sw()
        ]);
    }
}
