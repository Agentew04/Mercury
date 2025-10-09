using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Instructions;

/// <summary>
/// Jump to register instruction. Jumps to the address stored in the register provided.
/// </summary>
[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(20,16,0)] // rt
[FormatExact<Instruction>(15,11,0)] // rd
[FormatExact<Instruction>(10,6,0)] // shift
[FormatExact<Instruction>(5,0,8)] // funct
public partial class Jr : TypeRInstruction {

    public Jr() {
        OpCode = 0b000000;
        Rt = 0;
        Rd = 0;
        ShiftAmount = 0;
        Function = 0b001_000;
    }

    [GeneratedRegex(@"^\s*jr\s+\$(?<rs>\S+)\s*$")]
    public override partial Regex GetRegularExpression();

    public override void PopulateFromLine(string line) {
        Match m = GetRegularExpression().Match(line);
        Rs = byte.Parse(m.Groups["rs"].Value);
    }
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}" + FormatTrivia();
}
