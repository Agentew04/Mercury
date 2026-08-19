using Mercury.Engine.Common;
using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Mips.Runtime.Simple; 

public partial class Monocycle {

    private bool ExecuteTypeJ(IInstruction instruction) {
        if(instruction is J j) {
            isExecutingBranch = true;
            const uint pcMask = 0xF000_0000;
            branchAddress = 
                ((uint)Registers.Get(MipsGprRegisters.Pc) & pcMask) // PC[31..28]
                | ((uint)j.Immediate << 2);
            return true;
        }
        return false;
    }
}
