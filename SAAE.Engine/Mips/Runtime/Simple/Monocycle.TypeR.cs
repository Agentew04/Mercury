using SAAE.Engine.Mips.Instructions;

namespace SAAE.Engine.Mips.Runtime.Simple;

public partial class Monocycle {
    private void ExecuteTypeR(TypeRInstruction instruction) {
        if (instruction is Add add) {
            registerFile[add.Rd] = registerFile[add.Rs] + registerFile[add.Rt];
            if (IsOverflowed(registerFile[add.Rs], registerFile[add.Rt], registerFile[add.Rd])){
                OnSignalException?.Invoke(this, new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                    Instruction = add.ConvertToInt(),
                    ProgramCounter = registerFile[RegisterFile.Register.Pc]
                });
            }
        } else if (instruction is Addu addu) {
            registerFile[addu.Rd] = registerFile[addu.Rs] + registerFile[addu.Rt];
        } else if (instruction is Div div) {
            if (registerFile[div.Rt] == 0) {
                registerFile[RegisterFile.Register.Hi] = Random.Shared.Next(); // simulated undefined behaviour
                registerFile[RegisterFile.Register.Lo] = Random.Shared.Next();
            } else {
                registerFile[RegisterFile.Register.Hi] = registerFile[div.Rs] % registerFile[div.Rt];
                registerFile[RegisterFile.Register.Lo] = registerFile[div.Rs] / registerFile[div.Rt];
            }
        } else if (instruction is Divu divu) {
            if (registerFile[divu.Rt] == 0) {
                registerFile[RegisterFile.Register.Hi] = Random.Shared.Next(); // simulated undefined behaviour
                registerFile[RegisterFile.Register.Lo] = Random.Shared.Next();
            } else {
                registerFile[RegisterFile.Register.Hi] = registerFile[divu.Rs] % registerFile[divu.Rt];
                registerFile[RegisterFile.Register.Lo] = registerFile[divu.Rs] / registerFile[divu.Rt];
            }
        }// MADD e MADDU sao para o coproc1
        else if (instruction is Mfhi mfhi) {
            registerFile[mfhi.Rd] = registerFile[RegisterFile.Register.Hi];
        } else if (instruction is Mthi mthi) {
            registerFile[mthi.Rd] = registerFile[RegisterFile.Register.Hi];
        } else if (instruction is Mtlo mtlo) {
            registerFile[mtlo.Rd] = registerFile[RegisterFile.Register.Lo];
        } else if (instruction is Mflo mflo) {
            registerFile[mflo.Rd] = registerFile[RegisterFile.Register.Lo];
        } else if (instruction is Movn movn) {
            if (registerFile[movn.Rt] != 0) {
                registerFile[movn.Rd] = registerFile[movn.Rs];
            }
        } else if (instruction is Movz movz) {
            if (registerFile[movz.Rt] == 0) {
                registerFile[movz.Rd] = registerFile[movz.Rs];
            }
        } else if (instruction is Mul mul) {
            registerFile[mul.Rd] = registerFile[mul.Rs] * registerFile[mul.Rt];
        } else if (instruction is Mult mult) {
            long result = (long)registerFile[mult.Rs] * registerFile[mult.Rt];
            registerFile[RegisterFile.Register.Hi] = (int)(result >> 32);
            registerFile[RegisterFile.Register.Lo] = (int)(result & 0xFFFFFFFF);
        } else if (instruction is Multu multu) {
            ulong result = (ulong)registerFile[multu.Rs] * (ulong)registerFile[multu.Rt];
            registerFile[RegisterFile.Register.Hi] = (int)(result >> 32);
            registerFile[RegisterFile.Register.Lo] = (int)(result & 0xFFFFFFFF);
        } else if (instruction is Slt slt) {
            registerFile[slt.Rd] = registerFile[slt.Rs] < registerFile[slt.Rt] ? 1 : 0;
        } else if (instruction is Sltu sltu) {
            registerFile[sltu.Rd] = (uint)registerFile[sltu.Rs] < (uint)registerFile[sltu.Rt] ? 1 : 0;
        } else if (instruction is Sub sub) {
            int result = registerFile[sub.Rs] - registerFile[sub.Rt];
            if(IsOverflowed(registerFile[sub.Rs], registerFile[sub.Rt], result)) {
                // dont change rd, trap
                OnSignalException?.Invoke(this, new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                    Instruction = sub.ConvertToInt(),
                    ProgramCounter = registerFile[RegisterFile.Register.Pc],
                });
            } else {
                registerFile[sub.Rd] = result;
            }
        } else if (instruction is Subu subu) {
            registerFile[subu.Rd] = registerFile[subu.Rs] - registerFile[subu.Rt];
        } else if(instruction is Jalr1 or Jalr2) {
            registerFile[instruction.Rd] = registerFile[RegisterFile.Register.Pc];
            registerFile[RegisterFile.Register.Pc] = registerFile[instruction.Rs];
        } else if(instruction is And and) {
            registerFile[and.Rd] = registerFile[and.Rs] & registerFile[and.Rt];
        } else if(instruction is Nor nor) {
            registerFile[nor.Rd] = ~(registerFile[nor.Rs] | registerFile[nor.Rt]);
        } else if(instruction is Or or) {
            registerFile[or.Rd] = registerFile[or.Rs] | registerFile[or.Rt];
        } else if(instruction is Xor xor) {
            registerFile[xor.Rd] = registerFile[xor.Rs] ^ registerFile[xor.Rt];
        } else if(instruction is Sll sll) {
            registerFile[sll.Rd] = registerFile[sll.Rt] << sll.ShiftAmount;
        } else if(instruction is Sllv sllv) {
            registerFile[sllv.Rd] = registerFile[sllv.Rt] << registerFile[sllv.Rs];
        } else if(instruction is Sra sra) {
            int filling = (registerFile[sra.Rt] < 0 ? -1 : 0) << (32-sra.ShiftAmount);
            registerFile[sra.Rd] = filling | (registerFile[sra.Rt] >> sra.ShiftAmount);
        } else if(instruction is Srav srav) {
            int filling = (registerFile[srav.Rt] < 0 ? -1 : 0) << (32 - registerFile[srav.Rs]);
            registerFile[srav.Rd] = filling | (registerFile[srav.Rt] >> registerFile[srav.Rs]);
        } else if(instruction is Srl srl) {
            registerFile[srl.Rd] = (int)((uint)registerFile[srl.Rt] >> srl.ShiftAmount);
        } else if(instruction is Srlv srlv) {
            registerFile[srlv.Rd] = (int)((uint)registerFile[srlv.Rt] >> registerFile[srlv.Rs]);
        } else if(instruction is Break @break) {
            OnSignalException?.Invoke(this, new SignalExceptionEventArgs() {
                Signal = SignalExceptionEventArgs.SignalType.Breakpoint,
                Instruction = @break.ConvertToInt(),
                ProgramCounter = registerFile[RegisterFile.Register.Pc]
            });
        } else if(instruction is Syscall syscall) {
            OnSignalException?.Invoke(this, new SignalExceptionEventArgs() {
                Signal = SignalExceptionEventArgs.SignalType.SystemCall,
                Instruction = syscall.ConvertToInt(),
                ProgramCounter = registerFile[RegisterFile.Register.Pc]
            });
        } else if (instruction is Teq teq && registerFile[teq.Rs] == registerFile[teq.Rt]) {
            OnSignalException?.Invoke(this, new SignalExceptionEventArgs() {
                Signal = SignalExceptionEventArgs.SignalType.Trap,
                Instruction = teq.ConvertToInt(),
                ProgramCounter = registerFile[RegisterFile.Register.Pc]
            });
        }else if(instruction is Jr jr) {
            registerFile[RegisterFile.Register.Pc] = registerFile[jr.Rs];
        }
    }
}
