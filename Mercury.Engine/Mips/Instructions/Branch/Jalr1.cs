using System;
using System.Text.RegularExpressions;
using SAAE.Generators;

namespace Mercury.Engine.Mips.Instructions;

/// <summary>
/// Jump and link register. Jumps to the address stored in the register provided and stores the return address in $ra register or
/// an specific one.
/// </summary>
[FormatExact<Instruction>(31,26,0)] // opcode
[FormatExact<Instruction>(20,16,0)] // rt
[FormatExact<Instruction>(15,11,31)] // rd
[FormatExact<Instruction>(10,6,0)] // shift
[FormatExact<Instruction>(5,0,9)] // funct
public partial class Jalr1 : TypeRInstruction {

    public Jalr1() {
        OpCode = 0b000000;
        Rt = 0;
        Rd = 0b11111;
        ShiftAmount = 0;
        Function = 0b001_001;
        ParseOptions = PopulationOptions.Rs;
    }

    [GeneratedRegex(@"^\s*jalr\s+\s*\$(?<rs>[^\s,]+)\s*$")]
    public override partial Regex GetRegularExpression();
    
    public override string ToString() => $"{Mnemonic} ${TranslateRegisterName(Rs)}" + FormatTrivia();
}
