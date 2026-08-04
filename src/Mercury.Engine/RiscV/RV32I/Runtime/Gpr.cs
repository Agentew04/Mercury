using Mercury.Engine.Common;
using Mercury.Generators;

namespace Mercury.Engine.RiscV.RV32I.Runtime;

[RegisterGroupDefinition(Architecture.RiscV, Processor = 0, Name = "GPR", ProcessorName = "CPU")]
public enum Rv32Gpr
{
    [Register(0, "x0",32, true)]
    Zero,
    [Register(1, "x1",32, true)]
    X1,
    [Register(2, "x2",32, true)]
    X2,
    [Register(3, "x3",32, true)]
    X3,
    [Register(4, "x4",32, true)]
    X4,
    [Register(5, "x5",32, true)]
    X5,
    [Register(6, "x6",32, true)]
    X6,
    [Register(7, "x7",32, true)]
    X7,
    [Register(8, "x8",32, true)]
    X8,
    [Register(9, "x9",32, true)]
    X9,
    [Register(10, "x10",32, true)]
    X10,
    [Register(11, "x11",32, true)]
    X11,
    [Register(12, "x12",32, true)]
    X12,
    [Register(13, "x13",32, true)]
    X13,
    [Register(14, "x14",32, true)]
    X14,
    [Register(15, "x15",32, true)]
    X15,
    [Register(16, "x16",32, true)]
    X16,
    [Register(17, "x17",32, true)]
    X17,
    [Register(18, "x18",32, true)]
    X18,
    [Register(19, "x19",32, true)]
    X19,
    [Register(20, "x20",32, true)]
    X20,
    [Register(21, "x21",32, true)]
    X21,
    [Register(22, "x22",32, true)]
    X22,
    [Register(23, "x23",32, true)]
    X23,
    [Register(24, "x24",32, true)]
    X24,
    [Register(25, "x25",32, true)]
    X25,
    [Register(26, "x26",32, true)]
    X26,
    [Register(27, "x27",32, true)]
    X27,
    [Register(28, "x28",32, true)]
    X28,
    [Register(29, "x29",32, true)]
    X29,
    [Register(30, "x30",32, true)]
    X30,
    [Register(31, "x31",32, true)]
    X31,
    [Register("pc",32, false)]
    Pc
}