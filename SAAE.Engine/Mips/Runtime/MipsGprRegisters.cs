using SAAE.Engine.Common;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Runtime;

[RegisterInfoDefinition(Architecture.Mips)]
public enum MipsGprRegisters {
    [RegisterInfo(0, "zero",32)]
    Zero,
    [RegisterInfo(1, "at",32)]
    At,
    [RegisterInfo(2, "v0",32)]
    V0,
    [RegisterInfo(3, "v1",32)]
    V1,
    [RegisterInfo(4, "a0",32)]
    A0,
    [RegisterInfo(5, "a1",32)]
    A1,
    [RegisterInfo(6, "a2",32)]
    A2,
    [RegisterInfo(7, "a3",32)]
    A3,
    [RegisterInfo(8, "t0",32)]
    T0,
    [RegisterInfo(9, "t1",32)]
    T1,
    [RegisterInfo(10, "t2",32)]
    T2,
    [RegisterInfo(11, "t3",32)]
    T3,
    [RegisterInfo(12, "t4",32)]
    T4,
    [RegisterInfo(13, "t5",32)]
    T5,
    [RegisterInfo(14, "t6",32)]
    T6,
    [RegisterInfo(15, "t7",32)]
    T7,
    [RegisterInfo(16, "s0",32)]
    S0,
    [RegisterInfo(17, "s1",32)]
    S1,
    [RegisterInfo(18, "s2",32)]
    S2,
    [RegisterInfo(19, "s3",32)]
    S3,
    [RegisterInfo(20, "s4",32)]
    S4,
    [RegisterInfo(21, "s5",32)]
    S5,
    [RegisterInfo(22, "s6",32)]
    S6,
    [RegisterInfo(23, "s7",32)]
    S7,
    [RegisterInfo(24, "t8",32)]
    T8,
    [RegisterInfo(25, "t9",32)]
    T9,
    [RegisterInfo(26, "k0",32)]
    K0,
    [RegisterInfo(27, "k1",32)]
    K1,
    [RegisterInfo(28, "gp",32)]
    Gp,
    [RegisterInfo(29, "sp",32)]
    Sp,
    [RegisterInfo(30, "fp",32)]
    Fp,
    [RegisterInfo(31, "ra",32)]
    Ra,
    [RegisterInfo("pc",32)]
    Pc,
    [RegisterInfo("hi",32)]
    Hi,
    [RegisterInfo("lo",32)]
    Lo
}