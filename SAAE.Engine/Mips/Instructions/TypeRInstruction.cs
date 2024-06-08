using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Instructions;

public abstract class TypeRInstruction : Instruction {

    public byte OpCode { get; } = 0;

    public byte Rd { get; set; }

    public byte Rs { get; set; }

    public byte Rt { get; set; }

    public byte ShiftAmount { get; set; }

    public byte Function { get; protected set; }

    public override int ConvertToInt() {
        return ((OpCode & 0x3F) << 26) | ((Rs & 0x1F) << 21) | ((Rt & 0x1F) << 16) | ((Rd & 0x1F) << 11) | ((ShiftAmount & 0x1F) << 6) | (Function & 0x3F);
    }
}
