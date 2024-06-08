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
    }
}
