using SAAE.Engine.Common;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Runtime;

[RegisterInfoDefinition(Architecture.Mips)]
public enum MipsFpuRegisters {
    [RegisterInfo(0, "f0",32)]
    F0,
    [RegisterInfo(1, "f1",32)]
    F1,
    [RegisterInfo(2, "f2",32)]
    F2,
    [RegisterInfo(3, "f3",32)]
    F3,
    [RegisterInfo(4, "f4",32)]
    F4,
    [RegisterInfo(5, "f5",32)]
    F5,
    [RegisterInfo(6, "f6",32)]
    F6,
    [RegisterInfo(7, "f7",32)]
    F7,
    [RegisterInfo(8, "f8",32)]
    F8,
    [RegisterInfo(9, "f9",32)]
    F9,
    [RegisterInfo(10, "f10",32)]
    F10,
    [RegisterInfo(11, "f11",32)]
    F11,
    [RegisterInfo(12, "f12",32)]
    F12,
    [RegisterInfo(13, "f13",32)]
    F13,
    [RegisterInfo(14, "f14",32)]
    F14,
    [RegisterInfo(15, "f15",32)]
    F15,
    [RegisterInfo(16, "f16",32)]
    F16,
    [RegisterInfo(17, "f17",32)]
    F17,
    [RegisterInfo(18, "f18",32)]
    F18,
    [RegisterInfo(19, "f19",32)]
    F19,
    [RegisterInfo(20, "f20",32)]
    F20,
    [RegisterInfo(21, "f21",32)]
    F21,
    [RegisterInfo(22, "f22",32)]
    F22,
    [RegisterInfo(23, "f23",32)]
    F23,
    [RegisterInfo(24, "f24",32)]
    F24,
    [RegisterInfo(25, "f25",32)]
    F25,
    [RegisterInfo(26, "f26",32)]
    F26,
    [RegisterInfo(27, "f27",32)]
    F27,
    [RegisterInfo(28, "f28",32)]
    F28,
    [RegisterInfo(29, "f29",32)]
    F29,
    [RegisterInfo(30, "f30",32)]
    F30,
    [RegisterInfo(31, "f31",32)]
    F31
}