using SAAE.Engine.Mips.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Runtime.Simple;

public partial class Monocycle {

    private void ExecuteTypeI(TypeIInstruction instruction) {
        if (instruction is Addi addi) {
            int result = registerFile[addi.Rs] + addi.Immediate;
            if (IsOverflowed(registerFile[addi.Rs], addi.Immediate, result)) {
                OnSignalException?.Invoke(this, new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                    Instruction = addi.ConvertToInt(),
                    ProgramCounter = registerFile[RegisterFile.Register.Pc]
                });
                return;
            }
            registerFile[addi.Rt] = result;
        } else if (instruction is Addiu addiu) {
            registerFile[addiu.Rt] = registerFile[addiu.Rs] + addiu.Immediate;
        } else if (instruction is Slti slti) {
            registerFile[slti.Rt] = registerFile[slti.Rs] < slti.Immediate ? 1 : 0;
        } else if (instruction is Sltiu sltiu) {
            registerFile[sltiu.Rt] = (uint)registerFile[sltiu.Rs] < (ushort)sltiu.Immediate ? 1 : 0;
        } else if (instruction is Beq beq) {
            if (registerFile[beq.Rs] == registerFile[beq.Rt]) {
                BranchTo(beq.Immediate);
            }
        } else if (instruction is Bgez bgez) {
            if (registerFile[bgez.Rs] >= 0) {
                BranchTo(bgez.Immediate);
            }
        } else if (instruction is Bgezal bgezal) {
            if (registerFile[bgezal.Rs] >= 0) {
                BranchTo(bgezal.Immediate);
                Link();
            }
        } else if(instruction is Bgtz bgtz) {
            if (registerFile[bgtz.Rs] > 0) {
                BranchTo(bgtz.Immediate);
            }
        } else if(instruction is Blez blez) {
            if (registerFile[blez.Rs] <= 0) {
                BranchTo(blez.Immediate);
            }
        } else if(instruction is Bltz bltz) {
            if (registerFile[bltz.Rs] < 0) {
                BranchTo(bltz.Immediate);
            }
        } else if(instruction is Bltzal bltzal) {
            if (registerFile[bltzal.Rs] < 0) {
                BranchTo(bltzal.Immediate);
                Link();
            }
        } else if(instruction is Bne bne) {
            if (registerFile[bne.Rs] != registerFile[bne.Rt]) {
                BranchTo(bne.Immediate);
            }
        } else if(instruction is Andi andi) {
            registerFile[andi.Rt] = registerFile[andi.Rs] & ZeroExtend(andi.Immediate);
        } else if(instruction is Ori ori) {
            registerFile[ori.Rt] = registerFile[ori.Rs] | ZeroExtend(ori.Immediate);
        } else if (instruction is Xori xori) {
            registerFile[xori.Rt] = registerFile[xori.Rs] ^ ZeroExtend(xori.Immediate);
        } else if(instruction is Lb lb) {
            registerFile[lb.Rt] = (sbyte)memory.ReadByte((ulong)(registerFile[lb.Rs] + lb.Immediate));
        } else if(instruction is Lbu lbu) {
            registerFile[lbu.Rt] = memory.ReadByte((ulong)(registerFile[lbu.Rs] + lbu.Immediate));
        }else if(instruction is Lh lh) {
            int address = registerFile[lh.Rs] + lh.Immediate;
            if (address % 2 != 0) {
                OnSignalException?.Invoke(this, new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.AddressError,
                    Instruction = lh.ConvertToInt(),
                    ProgramCounter = registerFile[RegisterFile.Register.Pc]
                });
                return;
            }
            registerFile[lh.Rt] = (short)(memory.ReadWord((ulong)address) & 0xFFFF);
        } else if(instruction is Lhu lhu) {
            int address = registerFile[lhu.Rs] + lhu.Immediate;
            if (address % 2 != 0) {
                OnSignalException?.Invoke(this, new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.AddressError,
                    Instruction = lhu.ConvertToInt(),
                    ProgramCounter = registerFile[RegisterFile.Register.Pc]
                });
                return;
            }
            registerFile[lhu.Rt] = (ushort)(memory.ReadWord((ulong)address) & 0xFFFF);
        } else if(instruction is Lui lui) {
            registerFile[lui.Rt] = lui.Immediate << 16;
        } else if(instruction is Lw lw) {
            int address = registerFile[lw.Rs] + lw.Immediate;
            if((address & 0b11) != 0) {
                OnSignalException?.Invoke(this, new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.AddressError,
                    Instruction = lw.ConvertToInt(),
                    ProgramCounter = registerFile[RegisterFile.Register.Pc]
                });
                return;
            }
            registerFile[lw.Rt] = memory.ReadWord((ulong)address);
        } else if(instruction is Sb sb) {
            memory.WriteByte((ulong)(registerFile[sb.Rs] + sb.Immediate), (byte)registerFile[sb.Rt]);
        } else if(instruction is Sh sh) {
            int address = registerFile[sh.Rs] + sh.Immediate;
            if((address & 0b1) != 0){
                OnSignalException?.Invoke(this, new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.AddressError,
                    Instruction = sh.ConvertToInt(),
                    ProgramCounter = registerFile[RegisterFile.Register.Pc]
                });
                return;
            }
            // write two bytes
            memory.WriteByte((ulong)address, (byte)(registerFile[sh.Rt] >> 8));
            memory.WriteByte((ulong)(address + 1), (byte)(registerFile[sh.Rt] & 0xFF));
        } else if(instruction is Sw sw) {
            int address = registerFile[sw.Rs] + sw.Immediate;
            if ((address & 0b11) != 0) {
                OnSignalException?.Invoke(this, new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.AddressError,
                    Instruction = sw.ConvertToInt(),
                    ProgramCounter = registerFile[RegisterFile.Register.Pc]
                });
                return;
            }
            memory.WriteWord((ulong)address, registerFile[sw.Rt]);
        } else if(instruction is Teqi teqi) {
            if (registerFile[teqi.Rs] == teqi.Immediate) {
                OnSignalException?.Invoke(this, new SignalExceptionEventArgs() {
                    Signal = SignalExceptionEventArgs.SignalType.Trap,
                    Instruction = teqi.ConvertToInt(),
                    ProgramCounter = registerFile[RegisterFile.Register.Pc]
                });
            }
        } 
    }
}
