using Mercury.Engine.Common;
using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Mips.Runtime.Simple;

public partial class Monocycle {
    private async ValueTask<bool> ExecuteTypeR(IInstruction instruction) {
        switch (instruction) {
            case Add add: {
                int a = Registers.Get<MipsGprRegisters>(add.Rs);
                int b = Registers.Get<MipsGprRegisters>(add.Rt);
                int result = a+b;
                if (IsOverflowed(a,b,result)) {
                    await InvokeSignal(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                        Instruction = (int)add.ConvertToInt(),
                        ProgramCounter = Registers.Get(MipsGprRegisters.Pc)
                    });
                    break;
                }
                Registers.Set<MipsGprRegisters>(number: add.Rd, result);
                break;
            }
            case Addu addu: {
                Registers.Set<MipsGprRegisters>(number: addu.Rd, 
                    value: Registers.Get<MipsGprRegisters>(addu.Rs) + Registers.Get<MipsGprRegisters>(addu.Rt));
                break;
            }
            case Div div: {
                if (Registers.Get<MipsGprRegisters>(div.Rt) == 0) {
                    Registers.Set(MipsGprRegisters.Hi, Random.Shared.Next()); // simulated undefined behaviour
                    Registers.Set(MipsGprRegisters.Lo, Random.Shared.Next());
                } else {
                    Registers.Set(MipsGprRegisters.Hi, 
                        Registers.Get<MipsGprRegisters>(div.Rs) % Registers.Get<MipsGprRegisters>(div.Rt));
                    Registers.Set(MipsGprRegisters.Lo, 
                        Registers.Get<MipsGprRegisters>(div.Rs) / Registers.Get<MipsGprRegisters>(div.Rt));
                }
                break;
            }
            case Divu divu: {
                if (Registers.Get<MipsGprRegisters>(divu.Rt) == 0) {
                    Registers.Set(MipsGprRegisters.Hi, Random.Shared.Next()); // simulated undefined behaviour
                    Registers.Set(MipsGprRegisters.Lo, Random.Shared.Next());
                } else {
                    Registers.Set(MipsGprRegisters.Hi, (int)
                        ((uint)Registers.Get<MipsGprRegisters>(divu.Rs) % (uint)Registers.Get<MipsGprRegisters>(divu.Rt)));
                    Registers.Set(MipsGprRegisters.Lo, (int)
                        ((uint)Registers.Get<MipsGprRegisters>(divu.Rs) / (uint)Registers.Get<MipsGprRegisters>(divu.Rt)));
                }
                break;
            }
            case Mfhi mfhi: {
                Registers.Set<MipsGprRegisters>(mfhi.Rd, Registers.Get(MipsGprRegisters.Hi));
                break;
            }
            case Mthi mthi: {
                Registers.Set<MipsGprRegisters>(mthi.Rs, Registers.Get(MipsGprRegisters.Hi));
                break;
            }
            case Mtlo mtlo: {
                Registers.Set<MipsGprRegisters>(mtlo.Rs, Registers.Get(MipsGprRegisters.Lo));
                break;
            }
            case Mflo mflo: {
                Registers.Set<MipsGprRegisters>(mflo.Rd, Registers.Get(MipsGprRegisters.Lo));
                break;
            }
            case Movn movn: {
                if (Registers.Get<MipsGprRegisters>(movn.Rt) != 0) {
                    Registers.Set<MipsGprRegisters>(movn.Rd, Registers.Get<MipsGprRegisters>(movn.Rs));
                }
                break;
            }
            case Movz movz: {
                if (Registers.Get<MipsGprRegisters>(movz.Rt) == 0) {
                    Registers.Set<MipsGprRegisters>(movz.Rd, Registers.Get<MipsGprRegisters>(movz.Rs));
                }
                break;
            }
            case Mult mult: {
                long result = (long)Registers.Get<MipsGprRegisters>(mult.Rs) * Registers.Get<MipsGprRegisters>(mult.Rt);
                Registers.Set(MipsGprRegisters.Hi, (int)(result >> 32));
                Registers.Set(MipsGprRegisters.Lo, (int)(result & 0xFFFFFFFF));
                break;
            }
            case Multu multu: {
                ulong result = (ulong)Registers.Get<MipsGprRegisters>(multu.Rs) * (ulong)Registers.Get<MipsGprRegisters>(multu.Rt);
                Registers.Set(MipsGprRegisters.Hi, (int)(result >> 32));
                Registers.Set(MipsGprRegisters.Lo, (int)(result & 0xFFFFFFFF));
                break;
            }
            case Slt slt: {
                Registers.Set<MipsGprRegisters>(slt.Rd, Registers.Get<MipsGprRegisters>(slt.Rs) < Registers.Get<MipsGprRegisters>(slt.Rt) ? 1 : 0);
                break;
            }
            case Sltu sltu: {
                Registers.Set<MipsGprRegisters>(sltu.Rd, (uint)Registers.Get<MipsGprRegisters>(sltu.Rs) < (uint)Registers.Get<MipsGprRegisters>(sltu.Rt) ? 1 : 0);
                break;
            }
            case Sub sub: {
                int a = Registers.Get<MipsGprRegisters>(sub.Rs);
                int b = Registers.Get<MipsGprRegisters>(sub.Rt);
                int result = a - b;
                if(IsOverflowed(a, b, result)) {
                    // dont change rd, trap
                    await InvokeSignal(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                        Instruction = (int)sub.ConvertToInt(),
                        ProgramCounter = Registers.Get(MipsGprRegisters.Pc),
                    });
                } else {
                    Registers.Set<MipsGprRegisters>(sub.Rd, result);
                }
                break;
            }
            case Subu subu: {
                Registers.Set<MipsGprRegisters>(subu.Rd, Registers.Get<MipsGprRegisters>(subu.Rs) - Registers.Get<MipsGprRegisters>(subu.Rt));
                break;
            }
            case Jalr jalr: {
                Registers.Set<MipsGprRegisters>(jalr.Rd, Registers.Get(MipsGprRegisters.Pc));
                Registers.Set(MipsGprRegisters.Pc, Registers.Get<MipsGprRegisters>(jalr.Rs));
                break;
            }
            case And and: {
                Registers.Set<MipsGprRegisters>(and.Rd, Registers.Get<MipsGprRegisters>(and.Rs) & Registers.Get<MipsGprRegisters>(and.Rt));
                break;
            }
            case Nor nor: {
                Registers.Set<MipsGprRegisters>(nor.Rd, ~(Registers.Get<MipsGprRegisters>(nor.Rs) | Registers.Get<MipsGprRegisters>(nor.Rt)));
                break;
            }
            case Or or: {
                Registers.Set<MipsGprRegisters>(or.Rd, Registers.Get<MipsGprRegisters>(or.Rs) | Registers.Get<MipsGprRegisters>(or.Rt));
                break;
            }
            case Xor xor: {
                Registers.Set<MipsGprRegisters>(xor.Rd, Registers.Get<MipsGprRegisters>(xor.Rs) ^ Registers.Get<MipsGprRegisters>(xor.Rt));
                break;
            }
            case Sll sll: {
                Registers.Set<MipsGprRegisters>(sll.Rd, Registers.Get<MipsGprRegisters>(sll.Rt) << sll.ShiftAmount);
                break;
            }
            case Sllv sllv: {
                Registers.Set<MipsGprRegisters>(sllv.Rd, Registers.Get<MipsGprRegisters>(sllv.Rt) << Registers.Get<MipsGprRegisters>(sllv.Rs));
                break;
            }
            case Sra sra: {
                int filling = (Registers.Get<MipsGprRegisters>(sra.Rt) < 0 ? -1 : 0) << (32-sra.ShiftAmount);
                Registers.Set<MipsGprRegisters>(sra.Rd, filling | (Registers.Get<MipsGprRegisters>(sra.Rt) >> sra.ShiftAmount));
                break;
            }
            case Srav srav: {
                int filling = (Registers.Get<MipsGprRegisters>(srav.Rt) < 0 ? -1 : 0) << (32 - Registers.Get<MipsGprRegisters>(srav.Rs));
                Registers.Set<MipsGprRegisters>(srav.Rd, filling | (Registers.Get<MipsGprRegisters>(srav.Rt) >> Registers.Get<MipsGprRegisters>(srav.Rs)));
                break;
            }
            case Srl srl: {
                Registers.Set<MipsGprRegisters>(srl.Rd, Registers.Get<MipsGprRegisters>(srl.Rt) >>> srl.ShiftAmount);
                break;
            }
            case Srlv srlv: {
                Registers.Set<MipsGprRegisters>(srlv.Rd, Registers.Get<MipsGprRegisters>(srlv.Rt) >>> Registers.Get<MipsGprRegisters>(srlv.Rs));
                break;
            }
            case Break @break: {
                await InvokeSignal(new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.Breakpoint,
                    Instruction = (int)@break.ConvertToInt(),
                    ProgramCounter = Registers.Get(MipsGprRegisters.Pc)
                });
                break;
            }
            case Syscall syscall: {
                await InvokeSignal(new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.SystemCall,
                    Instruction = (int)syscall.ConvertToInt(),
                    ProgramCounter = Registers.Get(MipsGprRegisters.Pc)
                });
                break;
            }
            case Teq teq: {
                if (Registers.Get<MipsGprRegisters>(teq.Rs) == Registers.Get<MipsGprRegisters>(teq.Rt)) {
                    await InvokeSignal(new SignalExceptionEventArgs {
                        Signal = SignalExceptionEventArgs.SignalType.Trap,
                        Instruction = (int)teq.ConvertToInt(),
                        ProgramCounter = Registers.Get(MipsGprRegisters.Pc)
                    });
                }
                break;
            }
            case Jr jr: {
                Registers.Set(MipsGprRegisters.Pc, Registers.Get<MipsGprRegisters>(jr.Rs));
                break;
            }
            default: 
                return false;
        }
        return true;
    }

    
}
