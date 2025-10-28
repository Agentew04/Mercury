using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Mips.Runtime.Simple;

public partial class Monocycle {
    private async ValueTask ExecuteTypeR(TypeRInstruction instruction) {
        if (instruction is Add add) {
            RegisterBank.Set<MipsGprRegisters>(add.Rd, RegisterBank.Get<MipsGprRegisters>(add.Rs) + RegisterBank.Get<MipsGprRegisters>(add.Rt));
            if (IsOverflowed(RegisterBank.Get<MipsGprRegisters>(add.Rs), RegisterBank.Get<MipsGprRegisters>(add.Rt), RegisterBank.Get<MipsGprRegisters>(add.Rd))){
                if (OnSignalException is not null) {
                    await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                        Instruction = add.ConvertToInt(),
                        ProgramCounter = RegisterBank[MipsGprRegisters.Pc]
                    });
                }
            }
        } else if (instruction is Addu addu) {
            RegisterBank.Set<MipsGprRegisters>(addu.Rd, RegisterBank.Get<MipsGprRegisters>(addu.Rs) + RegisterBank.Get<MipsGprRegisters>(addu.Rt));
        } else if (instruction is Div div) {
            if (RegisterBank.Get<MipsGprRegisters>(div.Rt) == 0) {
                RegisterBank[MipsGprRegisters.Hi] = Random.Shared.Next(); // simulated undefined behaviour
                RegisterBank[MipsGprRegisters.Lo] = Random.Shared.Next();
            } else {
                RegisterBank[MipsGprRegisters.Hi] = RegisterBank.Get<MipsGprRegisters>(div.Rs) % RegisterBank.Get<MipsGprRegisters>(div.Rt);
                RegisterBank[MipsGprRegisters.Lo] = RegisterBank.Get<MipsGprRegisters>(div.Rs) / RegisterBank.Get<MipsGprRegisters>(div.Rt);
            }
        } else if (instruction is Divu divu) {
            if (RegisterBank.Get<MipsGprRegisters>(divu.Rt) == 0) {
                RegisterBank[MipsGprRegisters.Hi] = Random.Shared.Next(); // simulated undefined behaviour
                RegisterBank[MipsGprRegisters.Lo] = Random.Shared.Next();
            } else {
                RegisterBank[MipsGprRegisters.Hi] = RegisterBank.Get<MipsGprRegisters>(divu.Rs) % RegisterBank.Get<MipsGprRegisters>(divu.Rt);
                RegisterBank[MipsGprRegisters.Lo] = RegisterBank.Get<MipsGprRegisters>(divu.Rs) / RegisterBank.Get<MipsGprRegisters>(divu.Rt);
            }
        }// MADD e MADDU sao para o coproc1
        else if (instruction is Mfhi mfhi) {
            RegisterBank.Set<MipsGprRegisters>(mfhi.Rd, RegisterBank[MipsGprRegisters.Hi]);
        } else if (instruction is Mthi mthi) {
            RegisterBank.Set<MipsGprRegisters>(mthi.Rd, RegisterBank[MipsGprRegisters.Hi]);
        } else if (instruction is Mtlo mtlo) {
            RegisterBank.Set<MipsGprRegisters>(mtlo.Rd, RegisterBank[MipsGprRegisters.Lo]);
        } else if (instruction is Mflo mflo) {
            RegisterBank.Set<MipsGprRegisters>(mflo.Rd, RegisterBank[MipsGprRegisters.Lo]);
        } else if (instruction is Movn movn) {
            if (RegisterBank.Get<MipsGprRegisters>(movn.Rt) != 0) {
                RegisterBank.Set<MipsGprRegisters>(movn.Rd, RegisterBank.Get<MipsGprRegisters>(movn.Rs));
            }
        } else if (instruction is Movz movz) {
            if (RegisterBank.Get<MipsGprRegisters>(movz.Rt) == 0) {
                RegisterBank.Set<MipsGprRegisters>(movz.Rd, RegisterBank.Get<MipsGprRegisters>(movz.Rs));
            }
        } else if (instruction is Mul mul) {
            RegisterBank.Set<MipsGprRegisters>(mul.Rd, RegisterBank.Get<MipsGprRegisters>(mul.Rs) * RegisterBank.Get<MipsGprRegisters>(mul.Rt));
        } else if (instruction is Mult mult) {
            long result = (long)RegisterBank.Get<MipsGprRegisters>(mult.Rs) * RegisterBank.Get<MipsGprRegisters>(mult.Rt);
            RegisterBank[MipsGprRegisters.Hi] = (int)(result >> 32);
            RegisterBank[MipsGprRegisters.Lo] = (int)(result & 0xFFFFFFFF);
        } else if (instruction is Multu multu) {
            ulong result = (ulong)RegisterBank.Get<MipsGprRegisters>(multu.Rs) * (ulong)RegisterBank.Get<MipsGprRegisters>(multu.Rt);
            RegisterBank[MipsGprRegisters.Hi] = (int)(result >> 32);
            RegisterBank[MipsGprRegisters.Lo] = (int)(result & 0xFFFFFFFF);
        } else if (instruction is Slt slt) {
            RegisterBank.Set<MipsGprRegisters>(slt.Rd, RegisterBank.Get<MipsGprRegisters>(slt.Rs) < RegisterBank.Get<MipsGprRegisters>(slt.Rt) ? 1 : 0);
        } else if (instruction is Sltu sltu) {
            RegisterBank.Set<MipsGprRegisters>(sltu.Rd, (uint)RegisterBank.Get<MipsGprRegisters>(sltu.Rs) < (uint)RegisterBank.Get<MipsGprRegisters>(sltu.Rt) ? 1 : 0);
        } else if (instruction is Sub sub) {
            int result = RegisterBank.Get<MipsGprRegisters>(sub.Rs) - RegisterBank.Get<MipsGprRegisters>(sub.Rt);
            if(IsOverflowed(RegisterBank.Get<MipsGprRegisters>(sub.Rs), RegisterBank.Get<MipsGprRegisters>(sub.Rt), result)) {
                // dont change rd, trap
                if (OnSignalException is not null) {
                    await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                        Instruction = sub.ConvertToInt(),
                        ProgramCounter = RegisterBank[MipsGprRegisters.Pc],
                    });
                }
            } else {
                RegisterBank.Set<MipsGprRegisters>(sub.Rd, result);
            }
        } else if (instruction is Subu subu) {
            RegisterBank.Set<MipsGprRegisters>(subu.Rd, RegisterBank.Get<MipsGprRegisters>(subu.Rs) - RegisterBank.Get<MipsGprRegisters>(subu.Rt));
        } else if(instruction is Jalr1 or Jalr2) {
            RegisterBank.Set<MipsGprRegisters>(instruction.Rd, RegisterBank[MipsGprRegisters.Pc]);
            RegisterBank[MipsGprRegisters.Pc] = RegisterBank.Get<MipsGprRegisters>(instruction.Rs);
        } else if(instruction is And and) {
            RegisterBank.Set<MipsGprRegisters>(and.Rd, RegisterBank.Get<MipsGprRegisters>(and.Rs) & RegisterBank.Get<MipsGprRegisters>(and.Rt));
        } else if(instruction is Nor nor) {
            RegisterBank.Set<MipsGprRegisters>(nor.Rd, ~(RegisterBank.Get<MipsGprRegisters>(nor.Rs) | RegisterBank.Get<MipsGprRegisters>(nor.Rt)));
        } else if(instruction is Or or) {
            RegisterBank.Set<MipsGprRegisters>(or.Rd, RegisterBank.Get<MipsGprRegisters>(or.Rs) | RegisterBank.Get<MipsGprRegisters>(or.Rt));
        } else if(instruction is Xor xor) {
            RegisterBank.Set<MipsGprRegisters>(xor.Rd, RegisterBank.Get<MipsGprRegisters>(xor.Rs) ^ RegisterBank.Get<MipsGprRegisters>(xor.Rt));
        } else if(instruction is Sll sll) {
            RegisterBank.Set<MipsGprRegisters>(sll.Rd, RegisterBank.Get<MipsGprRegisters>(sll.Rt) << sll.ShiftAmount);
        } else if(instruction is Sllv sllv) {
            RegisterBank.Set<MipsGprRegisters>(sllv.Rd, RegisterBank.Get<MipsGprRegisters>(sllv.Rt) << RegisterBank.Get<MipsGprRegisters>(sllv.Rs));
        } else if(instruction is Sra sra) {
            int filling = (RegisterBank.Get<MipsGprRegisters>(sra.Rt) < 0 ? -1 : 0) << (32-sra.ShiftAmount);
            RegisterBank.Set<MipsGprRegisters>(sra.Rd, filling | (RegisterBank.Get<MipsGprRegisters>(sra.Rt) >> sra.ShiftAmount));
        } else if(instruction is Srav srav) {
            int filling = (RegisterBank.Get<MipsGprRegisters>(srav.Rt) < 0 ? -1 : 0) << (32 - RegisterBank.Get<MipsGprRegisters>(srav.Rs));
            RegisterBank.Set<MipsGprRegisters>(srav.Rd, filling | (RegisterBank.Get<MipsGprRegisters>(srav.Rt) >> RegisterBank.Get<MipsGprRegisters>(srav.Rs)));
        } else if(instruction is Srl srl) {
            RegisterBank.Set<MipsGprRegisters>(srl.Rd, RegisterBank.Get<MipsGprRegisters>(srl.Rt) >>> srl.ShiftAmount);
        } else if(instruction is Srlv srlv) {
            RegisterBank.Set<MipsGprRegisters>(srlv.Rd, RegisterBank.Get<MipsGprRegisters>(srlv.Rt) >>> RegisterBank.Get<MipsGprRegisters>(srlv.Rs));
        } else if(instruction is Break @break) {
            if (OnSignalException is not null) {
                await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.Breakpoint,
                    Instruction = @break.ConvertToInt(),
                    ProgramCounter = RegisterBank[MipsGprRegisters.Pc]
                });
            }
        } else if(instruction is Syscall syscall) {
            if (OnSignalException is not null) {
                await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.SystemCall,
                    Instruction = syscall.ConvertToInt(),
                    ProgramCounter = RegisterBank[MipsGprRegisters.Pc]
                });
            }
        } else if (instruction is Teq teq && RegisterBank.Get<MipsGprRegisters>(teq.Rs) == RegisterBank.Get<MipsGprRegisters>(teq.Rt)) {
            if (OnSignalException is not null) {
                await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.Trap,
                    Instruction = teq.ConvertToInt(),
                    ProgramCounter = RegisterBank[MipsGprRegisters.Pc]
                });
            }
        }else if(instruction is Jr jr) {
            RegisterBank[MipsGprRegisters.Pc] = RegisterBank.Get<MipsGprRegisters>(jr.Rs);
        }
    }
}
