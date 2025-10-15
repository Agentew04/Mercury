using SAAE.Engine.Common;
using SAAE.Generators;

namespace SAAE.Engine.Mips.Runtime;

[RegisterBankDefinition(Architecture.Mips, Processor = 0)]
public enum MipsGprRegisters {
    [Register(0, "zero",32, false)]
    Zero,
    [Register(1, "at",32, false)]
    At,
    [Register(2, "v0",32, true)]
    V0,
    [Register(3, "v1",32,true)]
    V1,
    [Register(4, "a0",32,true)]
    A0,
    [Register(5, "a1",32,true)]
    A1,
    [Register(6, "a2",32,true)]
    A2,
    [Register(7, "a3",32,true)]
    A3,
    [Register(8, "t0",32,true)]
    T0,
    [Register(9, "t1",32,true)]
    T1,
    [Register(10, "t2",32,true)]
    T2,
    [Register(11, "t3",32,true)]
    T3,
    [Register(12, "t4",32,true)]
    T4,
    [Register(13, "t5",32,true)]
    T5,
    [Register(14, "t6",32,true)]
    T6,
    [Register(15, "t7",32,true)]
    T7,
    [Register(16, "s0",32,true)]
    S0,
    [Register(17, "s1",32,true)]
    S1,
    [Register(18, "s2",32,true)]
    S2,
    [Register(19, "s3",32,true)]
    S3,
    [Register(20, "s4",32,true)]
    S4,
    [Register(21, "s5",32,true)]
    S5,
    [Register(22, "s6",32,true)]
    S6,
    [Register(23, "s7",32,true)]
    S7,
    [Register(24, "t8",32,true)]
    T8,
    [Register(25, "t9",32,true)]
    T9,
    [Register(26, "k0",32, false)]
    K0,
    [Register(27, "k1",32,false)]
    K1,
    [Register(28, "gp",32,false)]
    Gp,
    [Register(29, "sp",32,false)]
    Sp,
    [Register(30, "fp",32,false)]
    Fp,
    [Register(31, "ra",32,false)]
    Ra,
    [Register("pc",32,false)]
    Pc,
    [Register("hi",32,false)]
    Hi,
    [Register("lo",32,false)]
    Lo
}