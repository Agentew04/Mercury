using Mercury.Engine.Common;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Runtime;

[RegisterBankDefinition(Architecture.Mips, Processor = 1)]
public enum MipsFpuRegisters {
    [Register(0, "f0",32,true)]
    F0,
    [Register(1, "f1",32,true)]
    F1,
    [Register(2, "f2",32,true)]
    F2,
    [Register(3, "f3",32,true)]
    F3,
    [Register(4, "f4",32,true)]
    F4,
    [Register(5, "f5",32,true)]
    F5,
    [Register(6, "f6",32,true)]
    F6,
    [Register(7, "f7",32,true)]
    F7,
    [Register(8, "f8",32,true)]
    F8,
    [Register(9, "f9",32,true)]
    F9,
    [Register(10, "f10",32,true)]
    F10,
    [Register(11, "f11",32,true)]
    F11,
    [Register(12, "f12",32,true)]
    F12,
    [Register(13, "f13",32,true)]
    F13,
    [Register(14, "f14",32,true)]
    F14,
    [Register(15, "f15",32,true)]
    F15,
    [Register(16, "f16",32,true)]
    F16,
    [Register(17, "f17",32,true)]
    F17,
    [Register(18, "f18",32,true)]
    F18,
    [Register(19, "f19",32,true)]
    F19,
    [Register(20, "f20",32,true)]
    F20,
    [Register(21, "f21",32,true)]
    F21,
    [Register(22, "f22",32,true)]
    F22,
    [Register(23, "f23",32,true)]
    F23,
    [Register(24, "f24",32,true)]
    F24,
    [Register(25, "f25",32,true)]
    F25,
    [Register(26, "f26",32,true)]
    F26,
    [Register(27, "f27",32,true)]
    F27,
    [Register(28, "f28",32,true)]
    F28,
    [Register(29, "f29",32,true)]
    F29,
    [Register(30, "f30",32,true)]
    F30,
    [Register(31, "f31",32,true)]
    F31
}