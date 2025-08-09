using SAAE.Engine.Mips.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Runtime.Simple;

public partial class Monocycle {

    private async ValueTask ExecuteTypeI(TypeIInstruction instruction) {
        if (instruction is Addi addi) {
            int result = RegisterFile[addi.Rs] + addi.Immediate;
            if (IsOverflowed(RegisterFile[addi.Rs], addi.Immediate, result)) {
                if (OnSignalException is not null) {
                    await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                        Instruction = addi.ConvertToInt(),
                        ProgramCounter = RegisterFile[RegisterFile.Register.Pc]
                    });
                }
                return;
            }
            RegisterFile[addi.Rt] = result;
        } else if (instruction is Addiu addiu) {
            RegisterFile[addiu.Rt] = RegisterFile[addiu.Rs] + addiu.Immediate;
        } else if (instruction is Slti slti) {
            RegisterFile[slti.Rt] = RegisterFile[slti.Rs] < slti.Immediate ? 1 : 0;
        } else if (instruction is Sltiu sltiu) {
            RegisterFile[sltiu.Rt] = (uint)RegisterFile[sltiu.Rs] < (ushort)sltiu.Immediate ? 1 : 0;
        } else if (instruction is Beq beq) {
            if (RegisterFile[beq.Rs] == RegisterFile[beq.Rt]) {
                BranchTo(beq.Immediate);
            }
        } else if (instruction is Bgez bgez) {
            if (RegisterFile[bgez.Rs] >= 0) {
                BranchTo(bgez.Immediate);
            }
        } else if (instruction is Bgezal bgezal) {
            if (RegisterFile[bgezal.Rs] >= 0) {
                BranchTo(bgezal.Immediate);
                Link();
            }
        } else if(instruction is Bgtz bgtz) {
            if (RegisterFile[bgtz.Rs] > 0) {
                BranchTo(bgtz.Immediate);
            }
        } else if(instruction is Blez blez) {
            if (RegisterFile[blez.Rs] <= 0) {
                BranchTo(blez.Immediate);
            }
        } else if(instruction is Bltz bltz) {
            if (RegisterFile[bltz.Rs] < 0) {
                BranchTo(bltz.Immediate);
            }
        } else if(instruction is Bltzal bltzal) {
            if (RegisterFile[bltzal.Rs] < 0) {
                BranchTo(bltzal.Immediate);
                Link();
            }
        } else if(instruction is Bne bne) {
            if (RegisterFile[bne.Rs] != RegisterFile[bne.Rt]) {
                BranchTo(bne.Immediate);
            }
        } else if(instruction is Andi andi) {
            RegisterFile[andi.Rt] = RegisterFile[andi.Rs] & ZeroExtend(andi.Immediate);
        } else if(instruction is Ori ori) {
            RegisterFile[ori.Rt] = RegisterFile[ori.Rs] | ZeroExtend(ori.Immediate);
        } else if (instruction is Xori xori) {
            RegisterFile[xori.Rt] = RegisterFile[xori.Rs] ^ ZeroExtend(xori.Immediate);
        } else if(instruction is Lb lb) {
            RegisterFile[lb.Rt] = (sbyte)Memory.ReadByte((ulong)(RegisterFile[lb.Rs] + lb.Immediate));
            Machine.InvokeMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)(RegisterFile[lb.Rs] + lb.Immediate),
                Size = 1,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Lbu lbu) {
            RegisterFile[lbu.Rt] = Memory.ReadByte((ulong)(RegisterFile[lbu.Rs] + lbu.Immediate));
            Machine.InvokeMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)(RegisterFile[lbu.Rs] + lbu.Immediate),
                Size = 1,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        }else if(instruction is Lh lh) {
            int address = RegisterFile[lh.Rs] + lh.Immediate;
            if (address % 2 != 0) {
                if (OnSignalException is not null) {
                    await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = lh.ConvertToInt(),
                        ProgramCounter = RegisterFile[RegisterFile.Register.Pc]
                    });
                }
                return;
            }
            RegisterFile[lh.Rt] = (short)(Memory.ReadWord((ulong)address) & 0xFFFF);
            Machine.InvokeMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 2,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Lhu lhu) {
            int address = RegisterFile[lhu.Rs] + lhu.Immediate;
            if (address % 2 != 0) {
                if (OnSignalException is not null) {
                    await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = lhu.ConvertToInt(),
                        ProgramCounter = RegisterFile[RegisterFile.Register.Pc]
                    });
                }
                return;
            }
            RegisterFile[lhu.Rt] = (ushort)(Memory.ReadWord((ulong)address) & 0xFFFF);
            Machine.InvokeMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 2,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Lui lui) {
            RegisterFile[lui.Rt] = lui.Immediate << 16;
        } else if(instruction is Lw lw) {
            int address = RegisterFile[lw.Rs] + lw.Immediate;
            if((address & 0b11) != 0) {
                if (OnSignalException is not null) {
                    await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = lw.ConvertToInt(),
                        ProgramCounter = RegisterFile[RegisterFile.Register.Pc]
                    });
                }
                return;
            }
            RegisterFile[lw.Rt] = Memory.ReadWord((ulong)address);
            Machine.InvokeMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 4,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Sb sb) {
            Memory.WriteByte((ulong)(RegisterFile[sb.Rs] + sb.Immediate), (byte)RegisterFile[sb.Rt]);
            Machine.InvokeMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)(RegisterFile[sb.Rs] + sb.Immediate),
                Size = 1,
                Mode = MemoryAccessMode.Write,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Sh sh) {
            int address = RegisterFile[sh.Rs] + sh.Immediate;
            if((address & 0b1) != 0){
                if (OnSignalException is not null) {
                    await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = sh.ConvertToInt(),
                        ProgramCounter = RegisterFile[RegisterFile.Register.Pc]
                    });
                }
                return;
            }
            // write two bytes
            Memory.WriteByte((ulong)address, (byte)(RegisterFile[sh.Rt] >> 8));
            Memory.WriteByte((ulong)(address + 1), (byte)(RegisterFile[sh.Rt] & 0xFF));
            Machine.InvokeMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 2,
                Mode = MemoryAccessMode.Write,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Sw sw) {
            int address = RegisterFile[sw.Rs] + sw.Immediate;
            if ((address & 0b11) != 0) {
                if (OnSignalException is not null) {
                    await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = sw.ConvertToInt(),
                        ProgramCounter = RegisterFile[RegisterFile.Register.Pc]
                    });
                }
                return;
            }
            Memory.WriteWord((ulong)address, RegisterFile[sw.Rt]);
            Machine.InvokeMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 4,
                Mode = MemoryAccessMode.Write,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Teqi teqi) {
            if (RegisterFile[teqi.Rs] == teqi.Immediate) {
                if (OnSignalException is not null) {
                    await OnSignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.Trap,
                        Instruction = teqi.ConvertToInt(),
                        ProgramCounter = RegisterFile[RegisterFile.Register.Pc]
                    });
                }
            }
        } 
    }
}
