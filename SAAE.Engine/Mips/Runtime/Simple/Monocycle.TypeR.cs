using SAAE.Engine.Mips.Instructions;

namespace SAAE.Engine.Mips.Runtime.Simple;

public partial class Monocycle {
    private async ValueTask ExecuteTypeR(TypeRInstruction instruction) {
        if (instruction is Add add) {
            RegisterFile[add.Rd] = RegisterFile[add.Rs] + RegisterFile[add.Rt];
            if (IsOverflowed(RegisterFile[add.Rs], RegisterFile[add.Rt], RegisterFile[add.Rd])){
                if (OnSignalException is not null) {
                    await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                        Instruction = add.ConvertToInt(),
                        ProgramCounter = RegisterFile[RegisterFile.Register.Pc]
                    });
                }
            }
        } else if (instruction is Addu addu) {
            RegisterFile[addu.Rd] = RegisterFile[addu.Rs] + RegisterFile[addu.Rt];
        } else if (instruction is Div div) {
            if (RegisterFile[div.Rt] == 0) {
                RegisterFile[RegisterFile.Register.Hi] = Random.Shared.Next(); // simulated undefined behaviour
                RegisterFile[RegisterFile.Register.Lo] = Random.Shared.Next();
            } else {
                RegisterFile[RegisterFile.Register.Hi] = RegisterFile[div.Rs] % RegisterFile[div.Rt];
                RegisterFile[RegisterFile.Register.Lo] = RegisterFile[div.Rs] / RegisterFile[div.Rt];
            }
        } else if (instruction is Divu divu) {
            if (RegisterFile[divu.Rt] == 0) {
                RegisterFile[RegisterFile.Register.Hi] = Random.Shared.Next(); // simulated undefined behaviour
                RegisterFile[RegisterFile.Register.Lo] = Random.Shared.Next();
            } else {
                RegisterFile[RegisterFile.Register.Hi] = RegisterFile[divu.Rs] % RegisterFile[divu.Rt];
                RegisterFile[RegisterFile.Register.Lo] = RegisterFile[divu.Rs] / RegisterFile[divu.Rt];
            }
        }// MADD e MADDU sao para o coproc1
        else if (instruction is Mfhi mfhi) {
            RegisterFile[mfhi.Rd] = RegisterFile[RegisterFile.Register.Hi];
        } else if (instruction is Mthi mthi) {
            RegisterFile[mthi.Rd] = RegisterFile[RegisterFile.Register.Hi];
        } else if (instruction is Mtlo mtlo) {
            RegisterFile[mtlo.Rd] = RegisterFile[RegisterFile.Register.Lo];
        } else if (instruction is Mflo mflo) {
            RegisterFile[mflo.Rd] = RegisterFile[RegisterFile.Register.Lo];
        } else if (instruction is Movn movn) {
            if (RegisterFile[movn.Rt] != 0) {
                RegisterFile[movn.Rd] = RegisterFile[movn.Rs];
            }
        } else if (instruction is Movz movz) {
            if (RegisterFile[movz.Rt] == 0) {
                RegisterFile[movz.Rd] = RegisterFile[movz.Rs];
            }
        } else if (instruction is Mul mul) {
            RegisterFile[mul.Rd] = RegisterFile[mul.Rs] * RegisterFile[mul.Rt];
        } else if (instruction is Mult mult) {
            long result = (long)RegisterFile[mult.Rs] * RegisterFile[mult.Rt];
            RegisterFile[RegisterFile.Register.Hi] = (int)(result >> 32);
            RegisterFile[RegisterFile.Register.Lo] = (int)(result & 0xFFFFFFFF);
        } else if (instruction is Multu multu) {
            ulong result = (ulong)RegisterFile[multu.Rs] * (ulong)RegisterFile[multu.Rt];
            RegisterFile[RegisterFile.Register.Hi] = (int)(result >> 32);
            RegisterFile[RegisterFile.Register.Lo] = (int)(result & 0xFFFFFFFF);
        } else if (instruction is Slt slt) {
            RegisterFile[slt.Rd] = RegisterFile[slt.Rs] < RegisterFile[slt.Rt] ? 1 : 0;
        } else if (instruction is Sltu sltu) {
            RegisterFile[sltu.Rd] = (uint)RegisterFile[sltu.Rs] < (uint)RegisterFile[sltu.Rt] ? 1 : 0;
        } else if (instruction is Sub sub) {
            int result = RegisterFile[sub.Rs] - RegisterFile[sub.Rt];
            if(IsOverflowed(RegisterFile[sub.Rs], RegisterFile[sub.Rt], result)) {
                // dont change rd, trap
                if (OnSignalException is not null) {
                    await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                        Instruction = sub.ConvertToInt(),
                        ProgramCounter = RegisterFile[RegisterFile.Register.Pc],
                    });
                }
            } else {
                RegisterFile[sub.Rd] = result;
            }
        } else if (instruction is Subu subu) {
            RegisterFile[subu.Rd] = RegisterFile[subu.Rs] - RegisterFile[subu.Rt];
        } else if(instruction is Jalr1 or Jalr2) {
            RegisterFile[instruction.Rd] = RegisterFile[RegisterFile.Register.Pc];
            RegisterFile[RegisterFile.Register.Pc] = RegisterFile[instruction.Rs];
        } else if(instruction is And and) {
            RegisterFile[and.Rd] = RegisterFile[and.Rs] & RegisterFile[and.Rt];
        } else if(instruction is Nor nor) {
            RegisterFile[nor.Rd] = ~(RegisterFile[nor.Rs] | RegisterFile[nor.Rt]);
        } else if(instruction is Or or) {
            RegisterFile[or.Rd] = RegisterFile[or.Rs] | RegisterFile[or.Rt];
        } else if(instruction is Xor xor) {
            RegisterFile[xor.Rd] = RegisterFile[xor.Rs] ^ RegisterFile[xor.Rt];
        } else if(instruction is Sll sll) {
            RegisterFile[sll.Rd] = RegisterFile[sll.Rt] << sll.ShiftAmount;
        } else if(instruction is Sllv sllv) {
            RegisterFile[sllv.Rd] = RegisterFile[sllv.Rt] << RegisterFile[sllv.Rs];
        } else if(instruction is Sra sra) {
            int filling = (RegisterFile[sra.Rt] < 0 ? -1 : 0) << (32-sra.ShiftAmount);
            RegisterFile[sra.Rd] = filling | (RegisterFile[sra.Rt] >> sra.ShiftAmount);
        } else if(instruction is Srav srav) {
            int filling = (RegisterFile[srav.Rt] < 0 ? -1 : 0) << (32 - RegisterFile[srav.Rs]);
            RegisterFile[srav.Rd] = filling | (RegisterFile[srav.Rt] >> RegisterFile[srav.Rs]);
        } else if(instruction is Srl srl) {
            RegisterFile[srl.Rd] = RegisterFile[srl.Rt] >>> srl.ShiftAmount;
        } else if(instruction is Srlv srlv) {
            RegisterFile[srlv.Rd] = RegisterFile[srlv.Rt] >>> RegisterFile[srlv.Rs];
        } else if(instruction is Break @break) {
            if (OnSignalException is not null) {
                await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.Breakpoint,
                    Instruction = @break.ConvertToInt(),
                    ProgramCounter = RegisterFile[RegisterFile.Register.Pc]
                });
            }
        } else if(instruction is Syscall syscall) {
            if (OnSignalException is not null) {
                await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.SystemCall,
                    Instruction = syscall.ConvertToInt(),
                    ProgramCounter = RegisterFile[RegisterFile.Register.Pc]
                });
            }
        } else if (instruction is Teq teq && RegisterFile[teq.Rs] == RegisterFile[teq.Rt]) {
            if (OnSignalException is not null) {
                await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.Trap,
                    Instruction = teq.ConvertToInt(),
                    ProgramCounter = RegisterFile[RegisterFile.Register.Pc]
                });
            }
        }else if(instruction is Jr jr) {
            RegisterFile[RegisterFile.Register.Pc] = RegisterFile[jr.Rs];
        }
    }
}
