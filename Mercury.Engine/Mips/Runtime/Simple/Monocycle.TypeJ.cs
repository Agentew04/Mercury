using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Mips.Runtime.Simple; 

public partial class Monocycle {

    private void ExecuteTypeJ(TypeJInstruction instruction) {
        if(instruction is J j) {
            isExecutingBranch = true;
            const uint pcMask = 0xF000_0000;
            branchAddress = 
                ((uint)Registers[MipsGprRegisters.Pc] & pcMask) // PC[31..28]
                | ((uint)j.Immediate << 2);
        }else if(instruction is Jal jal)
        {
            isExecutingBranch = true;
            const uint pcMask = 0xF000_0000;
            branchAddress = 
                ((uint)Registers[MipsGprRegisters.Pc] & pcMask) // PC[31..28]
                | ((uint)jal.Immediate << 2);
            Link(); 
        }
    }
}
