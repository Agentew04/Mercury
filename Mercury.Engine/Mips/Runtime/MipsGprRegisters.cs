using Mercury.Engine.Common;
using Mercury.Generators;

namespace Mercury.Engine.Mips.Runtime;

[RegisterInfoDefinition(Architecture.Mips)]
public enum MipsGprRegisters {
    [RegisterInfo(0, "zero")]
    Zero,
    [RegisterInfo(1, "at")]
    At,
    [RegisterInfo(2, "v0")]
    V0,
    [RegisterInfo(3, "v1")]
    V1,
    [RegisterInfo(4, "a0")]
    A0,
    [RegisterInfo(5, "a1")]
    A1,
    [RegisterInfo(6, "a2")]
    A2,
    [RegisterInfo(7, "a3")]
    A3,
    [RegisterInfo(8, "t0")]
    T0,
    [RegisterInfo(9, "t1")]
    T1,
    [RegisterInfo(10, "t2")]
    T2,
    [RegisterInfo(11, "t3")]
    T3,
    [RegisterInfo(12, "t4")]
    T4,
    [RegisterInfo(13, "t5")]
    T5,
    [RegisterInfo(14, "t6")]
    T6,
    [RegisterInfo(15, "t7")]
    T7,
    [RegisterInfo(16, "s0")]
    S0,
    [RegisterInfo(17, "s1")]
    S1,
    [RegisterInfo(18, "s2")]
    S2,
    [RegisterInfo(19, "s3")]
    S3,
    [RegisterInfo(20, "s4")]
    S4,
    [RegisterInfo(21, "s5")]
    S5,
    [RegisterInfo(22, "s6")]
    S6,
    [RegisterInfo(23, "s7")]
    S7,
    [RegisterInfo(24, "t8")]
    T8,
    [RegisterInfo(25, "t9")]
    T9,
    [RegisterInfo(26, "k0")]
    K0,
    [RegisterInfo(27, "k1")]
    K1,
    [RegisterInfo(28, "gp")]
    Gp,
    [RegisterInfo(29, "sp")]
    Sp,
    [RegisterInfo(30, "fp")]
    Fp,
    [RegisterInfo(31, "ra")]
    Ra,
    [RegisterInfo("pc")]
    Pc,
    [RegisterInfo("hi")]
    Hi,
    [RegisterInfo("lo")]
    Lo
}