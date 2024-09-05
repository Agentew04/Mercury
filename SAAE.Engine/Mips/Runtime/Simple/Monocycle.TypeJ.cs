using SAAE.Engine.Mips.Instructions;

namespace SAAE.Engine.Mips.Runtime.Simple; 

public partial class Monocycle {

    private void ExecuteTypeJ(TypeJInstruction instruction) {
        if(instruction is J j) {
            uint pcMask = 0xF000_0000;
            RegisterFile[RegisterFile.Register.Pc] = (int)(
                ((uint)RegisterFile[RegisterFile.Register.Pc] & pcMask) // PC[31..28]
                | ((uint)j.Immediate << 2));
        }else if(instruction is Jal jal) {
            RegisterFile[RegisterFile.Register.Pc] = RegisterFile[RegisterFile.Register.Pc] + (UseBranchDelaySlot ? 8 : 4);
            uint pcMask = 0xF000_0000;
            RegisterFile[RegisterFile.Register.Pc] = (int)(
                ((uint)RegisterFile[RegisterFile.Register.Pc] & pcMask) // PC[31..28]
                | ((uint)jal.Immediate << 2));
        }
    }
}
