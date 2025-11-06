using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Mips.Runtime.Simple;

public partial class Monocycle {
    private async ValueTask ExecuteTypeR(TypeRInstruction instruction) {
        if (instruction is Add add) {
            Registers.Set<MipsGprRegisters>(add.Rd, Registers.Get<MipsGprRegisters>(add.Rs) + Registers.Get<MipsGprRegisters>(add.Rt));
            if (IsOverflowed(Registers.Get<MipsGprRegisters>(add.Rs), Registers.Get<MipsGprRegisters>(add.Rt), Registers.Get<MipsGprRegisters>(add.Rd))){
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                        Instruction = add.ConvertToInt(),
                        ProgramCounter = Registers[MipsGprRegisters.Pc]
                    });
                }
            }
        } else if (instruction is Addu addu) {
            Registers.Set<MipsGprRegisters>(addu.Rd, Registers.Get<MipsGprRegisters>(addu.Rs) + Registers.Get<MipsGprRegisters>(addu.Rt));
        } else if (instruction is Div div) {
            if (Registers.Get<MipsGprRegisters>(div.Rt) == 0) {
                Registers[MipsGprRegisters.Hi] = Random.Shared.Next(); // simulated undefined behaviour
                Registers[MipsGprRegisters.Lo] = Random.Shared.Next();
            } else {
                Registers[MipsGprRegisters.Hi] = Registers.Get<MipsGprRegisters>(div.Rs) % Registers.Get<MipsGprRegisters>(div.Rt);
                Registers[MipsGprRegisters.Lo] = Registers.Get<MipsGprRegisters>(div.Rs) / Registers.Get<MipsGprRegisters>(div.Rt);
            }
        } else if (instruction is Divu divu) {
            if (Registers.Get<MipsGprRegisters>(divu.Rt) == 0) {
                Registers[MipsGprRegisters.Hi] = Random.Shared.Next(); // simulated undefined behaviour
                Registers[MipsGprRegisters.Lo] = Random.Shared.Next();
            } else {
                Registers[MipsGprRegisters.Hi] = Registers.Get<MipsGprRegisters>(divu.Rs) % Registers.Get<MipsGprRegisters>(divu.Rt);
                Registers[MipsGprRegisters.Lo] = Registers.Get<MipsGprRegisters>(divu.Rs) / Registers.Get<MipsGprRegisters>(divu.Rt);
            }
        }// MADD e MADDU sao para o coproc1
        else if (instruction is Mfhi mfhi) {
            Registers.Set<MipsGprRegisters>(mfhi.Rd, Registers[MipsGprRegisters.Hi]);
        } else if (instruction is Mthi mthi) {
            Registers.Set<MipsGprRegisters>(mthi.Rd, Registers[MipsGprRegisters.Hi]);
        } else if (instruction is Mtlo mtlo) {
            Registers.Set<MipsGprRegisters>(mtlo.Rd, Registers[MipsGprRegisters.Lo]);
        } else if (instruction is Mflo mflo) {
            Registers.Set<MipsGprRegisters>(mflo.Rd, Registers[MipsGprRegisters.Lo]);
        } else if (instruction is Movn movn) {
            if (Registers.Get<MipsGprRegisters>(movn.Rt) != 0) {
                Registers.Set<MipsGprRegisters>(movn.Rd, Registers.Get<MipsGprRegisters>(movn.Rs));
            }
        } else if (instruction is Movz movz) {
            if (Registers.Get<MipsGprRegisters>(movz.Rt) == 0) {
                Registers.Set<MipsGprRegisters>(movz.Rd, Registers.Get<MipsGprRegisters>(movz.Rs));
            }
        } else if (instruction is Mul mul) {
            Registers.Set<MipsGprRegisters>(mul.Rd, Registers.Get<MipsGprRegisters>(mul.Rs) * Registers.Get<MipsGprRegisters>(mul.Rt));
        } else if (instruction is Mult mult) {
            long result = (long)Registers.Get<MipsGprRegisters>(mult.Rs) * Registers.Get<MipsGprRegisters>(mult.Rt);
            Registers[MipsGprRegisters.Hi] = (int)(result >> 32);
            Registers[MipsGprRegisters.Lo] = (int)(result & 0xFFFFFFFF);
        } else if (instruction is Multu multu) {
            ulong result = (ulong)Registers.Get<MipsGprRegisters>(multu.Rs) * (ulong)Registers.Get<MipsGprRegisters>(multu.Rt);
            Registers[MipsGprRegisters.Hi] = (int)(result >> 32);
            Registers[MipsGprRegisters.Lo] = (int)(result & 0xFFFFFFFF);
        } else if (instruction is Slt slt) {
            Registers.Set<MipsGprRegisters>(slt.Rd, Registers.Get<MipsGprRegisters>(slt.Rs) < Registers.Get<MipsGprRegisters>(slt.Rt) ? 1 : 0);
        } else if (instruction is Sltu sltu) {
            Registers.Set<MipsGprRegisters>(sltu.Rd, (uint)Registers.Get<MipsGprRegisters>(sltu.Rs) < (uint)Registers.Get<MipsGprRegisters>(sltu.Rt) ? 1 : 0);
        } else if (instruction is Sub sub) {
            int result = Registers.Get<MipsGprRegisters>(sub.Rs) - Registers.Get<MipsGprRegisters>(sub.Rt);
            if(IsOverflowed(Registers.Get<MipsGprRegisters>(sub.Rs), Registers.Get<MipsGprRegisters>(sub.Rt), result)) {
                // dont change rd, trap
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                        Instruction = sub.ConvertToInt(),
                        ProgramCounter = Registers[MipsGprRegisters.Pc],
                    });
                }
            } else {
                Registers.Set<MipsGprRegisters>(sub.Rd, result);
            }
        } else if (instruction is Subu subu) {
            Registers.Set<MipsGprRegisters>(subu.Rd, Registers.Get<MipsGprRegisters>(subu.Rs) - Registers.Get<MipsGprRegisters>(subu.Rt));
        } else if(instruction is Jalr1 or Jalr2) {
            Registers.Set<MipsGprRegisters>(instruction.Rd, Registers[MipsGprRegisters.Pc]);
            Registers[MipsGprRegisters.Pc] = Registers.Get<MipsGprRegisters>(instruction.Rs);
        } else if(instruction is And and) {
            Registers.Set<MipsGprRegisters>(and.Rd, Registers.Get<MipsGprRegisters>(and.Rs) & Registers.Get<MipsGprRegisters>(and.Rt));
        } else if(instruction is Nor nor) {
            Registers.Set<MipsGprRegisters>(nor.Rd, ~(Registers.Get<MipsGprRegisters>(nor.Rs) | Registers.Get<MipsGprRegisters>(nor.Rt)));
        } else if(instruction is Or or) {
            Registers.Set<MipsGprRegisters>(or.Rd, Registers.Get<MipsGprRegisters>(or.Rs) | Registers.Get<MipsGprRegisters>(or.Rt));
        } else if(instruction is Xor xor) {
            Registers.Set<MipsGprRegisters>(xor.Rd, Registers.Get<MipsGprRegisters>(xor.Rs) ^ Registers.Get<MipsGprRegisters>(xor.Rt));
        } else if(instruction is Sll sll) {
            Registers.Set<MipsGprRegisters>(sll.Rd, Registers.Get<MipsGprRegisters>(sll.Rt) << sll.ShiftAmount);
        } else if(instruction is Sllv sllv) {
            Registers.Set<MipsGprRegisters>(sllv.Rd, Registers.Get<MipsGprRegisters>(sllv.Rt) << Registers.Get<MipsGprRegisters>(sllv.Rs));
        } else if(instruction is Sra sra) {
            int filling = (Registers.Get<MipsGprRegisters>(sra.Rt) < 0 ? -1 : 0) << (32-sra.ShiftAmount);
            Registers.Set<MipsGprRegisters>(sra.Rd, filling | (Registers.Get<MipsGprRegisters>(sra.Rt) >> sra.ShiftAmount));
        } else if(instruction is Srav srav) {
            int filling = (Registers.Get<MipsGprRegisters>(srav.Rt) < 0 ? -1 : 0) << (32 - Registers.Get<MipsGprRegisters>(srav.Rs));
            Registers.Set<MipsGprRegisters>(srav.Rd, filling | (Registers.Get<MipsGprRegisters>(srav.Rt) >> Registers.Get<MipsGprRegisters>(srav.Rs)));
        } else if(instruction is Srl srl) {
            Registers.Set<MipsGprRegisters>(srl.Rd, Registers.Get<MipsGprRegisters>(srl.Rt) >>> srl.ShiftAmount);
        } else if(instruction is Srlv srlv) {
            Registers.Set<MipsGprRegisters>(srlv.Rd, Registers.Get<MipsGprRegisters>(srlv.Rt) >>> Registers.Get<MipsGprRegisters>(srlv.Rs));
        } else if(instruction is Break @break) {
            if (SignalException is not null) {
                await SignalException.Invoke(new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.Breakpoint,
                    Instruction = @break.ConvertToInt(),
                    ProgramCounter = Registers[MipsGprRegisters.Pc]
                });
            }
        } else if(instruction is Syscall syscall) {
            if (SignalException is not null) {
                await SignalException.Invoke(new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.SystemCall,
                    Instruction = syscall.ConvertToInt(),
                    ProgramCounter = Registers[MipsGprRegisters.Pc]
                });
            }
        } else if (instruction is Teq teq && Registers.Get<MipsGprRegisters>(teq.Rs) == Registers.Get<MipsGprRegisters>(teq.Rt)) {
            if (SignalException is not null) {
                await SignalException.Invoke(new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.Trap,
                    Instruction = teq.ConvertToInt(),
                    ProgramCounter = Registers[MipsGprRegisters.Pc]
                });
            }
        }else if(instruction is Jr jr) {
            Registers[MipsGprRegisters.Pc] = Registers.Get<MipsGprRegisters>(jr.Rs);
        }
    }
}
