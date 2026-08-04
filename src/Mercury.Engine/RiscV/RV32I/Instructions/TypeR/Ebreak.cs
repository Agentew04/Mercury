using Mercury.Engine.Common;
using Mercury.Engine.Generators.Instruction;

namespace Mercury.Engine.RiscV.RV32I.Instructions;

[Instruction]
[FormatExact(31,25,0)]
[FormatExact(24,20,0b00001)]
[FormatExact(19,7,0)]
[FormatExact(6,2,0b11100)]
[FormatExact(1,0,0b11)]
public partial class Ebreak : IInstruction;