using SAAE.Engine.Mips.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAAE.Engine.Common;

namespace SAAE.Engine.Mips.Runtime.Simple;

public partial class Monocycle {

    private async ValueTask ExecuteTypeI(TypeIInstruction instruction) {
        if (instruction is Addi addi) {
            int result = RegisterBank.Get<MipsGprRegisters>(addi.Rs) + addi.Immediate;
            if (IsOverflowed(RegisterBank.Get<MipsGprRegisters>(addi.Rs), addi.Immediate, result)) {
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                        Instruction = addi.ConvertToInt(),
                        ProgramCounter = RegisterBank.Get(MipsGprRegisters.Pc)
                    });
                }
                return;
            }
            RegisterBank.Set<MipsGprRegisters>(addi.Rt, result);
        } else if (instruction is Addiu addiu) {
            RegisterBank.Set<MipsGprRegisters>(addiu.Rt, RegisterBank.Get<MipsGprRegisters>(addiu.Rs) + addiu.Immediate);
        } else if (instruction is Slti slti) {
            RegisterBank.Set<MipsGprRegisters>(slti.Rt, RegisterBank.Get<MipsGprRegisters>(slti.Rs) < slti.Immediate ? 1 : 0);
        } else if (instruction is Sltiu sltiu) {
            RegisterBank.Set<MipsGprRegisters>(sltiu.Rt, (uint)RegisterBank.Get<MipsGprRegisters>(sltiu.Rs) < (ushort)sltiu.Immediate ? 1 : 0);
        } else if (instruction is Beq beq) {
            if (RegisterBank.Get<MipsGprRegisters>(beq.Rs) == RegisterBank.Get<MipsGprRegisters>(beq.Rt)) {
                BranchTo(beq.Immediate);
            }
        } else if (instruction is Bgez bgez) {
            if (RegisterBank.Get<MipsGprRegisters>(bgez.Rs) >= 0) {
                BranchTo(bgez.Immediate);
            }
        } else if (instruction is Bgezal bgezal) {
            if (RegisterBank.Get<MipsGprRegisters>(bgezal.Rs) >= 0) {
                BranchTo(bgezal.Immediate);
                Link();
            }
        } else if(instruction is Bgtz bgtz) {
            if (RegisterBank.Get<MipsGprRegisters>(bgtz.Rs) > 0) {
                BranchTo(bgtz.Immediate);
            }
        } else if(instruction is Blez blez) {
            if (RegisterBank.Get<MipsGprRegisters>(blez.Rs) <= 0) {
                BranchTo(blez.Immediate);
            }
        } else if(instruction is Bltz bltz) {
            if (RegisterBank.Get<MipsGprRegisters>(bltz.Rs) < 0) {
                BranchTo(bltz.Immediate);
            }
        } else if(instruction is Bltzal bltzal) {
            if (RegisterBank.Get<MipsGprRegisters>(bltzal.Rs) < 0) {
                BranchTo(bltzal.Immediate);
                Link();
            }
        } else if(instruction is Bne bne) {
            if (RegisterBank.Get<MipsGprRegisters>(bne.Rs) != RegisterBank.Get<MipsGprRegisters>(bne.Rt)) {
                BranchTo(bne.Immediate);
            }
        } else if(instruction is Andi andi) {
            RegisterBank.Set<MipsGprRegisters>(andi.Rt, RegisterBank.Get<MipsGprRegisters>(andi.Rs) & ZeroExtend(andi.Immediate));
        } else if(instruction is Ori ori) {
            RegisterBank.Set<MipsGprRegisters>(ori.Rt, RegisterBank.Get<MipsGprRegisters>(ori.Rs) | ZeroExtend(ori.Immediate));
        } else if (instruction is Xori xori) {
            RegisterBank.Set<MipsGprRegisters>(xori.Rt, RegisterBank.Get<MipsGprRegisters>(xori.Rs) ^ ZeroExtend(xori.Immediate));
        } else if(instruction is Lb lb) {
            RegisterBank.Set<MipsGprRegisters>(lb.Rt, (sbyte)MipsMachine.DataMemory.ReadByte((ulong)(RegisterBank.Get<MipsGprRegisters>(lb.Rs) + lb.Immediate)));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)(RegisterBank.Get<MipsGprRegisters>(lb.Rs) + lb.Immediate),
                Size = 1,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Lbu lbu) {
            RegisterBank.Set<MipsGprRegisters>(lbu.Rt, MipsMachine.DataMemory.ReadByte((ulong)(RegisterBank.Get<MipsGprRegisters>(lbu.Rs) + lbu.Immediate)));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)(RegisterBank.Get<MipsGprRegisters>(lbu.Rs) + lbu.Immediate),
                Size = 1,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        }else if(instruction is Lh lh) {
            int address = RegisterBank.Get<MipsGprRegisters>(lh.Rs) + lh.Immediate;
            if (address % 2 != 0) {
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = lh.ConvertToInt(),
                        ProgramCounter = RegisterBank[MipsGprRegisters.Pc]
                    });
                }
                return;
            }
            RegisterBank.Set<MipsGprRegisters>(lh.Rt, (short)(MipsMachine.DataMemory.ReadWord((ulong)address) & 0xFFFF));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 2,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Lhu lhu) {
            int address = RegisterBank.Get<MipsGprRegisters>(lhu.Rs) + lhu.Immediate;
            if (address % 2 != 0) {
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = lhu.ConvertToInt(),
                        ProgramCounter = RegisterBank[MipsGprRegisters.Pc]
                    });
                }
                return;
            }
            RegisterBank.Set<MipsGprRegisters>(lhu.Rt, (ushort)(MipsMachine.DataMemory.ReadWord((ulong)address) & 0xFFFF));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 2,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Lui lui) {
            RegisterBank.Set<MipsGprRegisters>(lui.Rt, lui.Immediate << 16);
        } else if(instruction is Lw lw) {
            int address = RegisterBank.Get<MipsGprRegisters>(lw.Rs) + lw.Immediate;
            if((address & 0b11) != 0) {
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = lw.ConvertToInt(),
                        ProgramCounter = RegisterBank[MipsGprRegisters.Pc]
                    });
                }
                return;
            }
            RegisterBank.Set<MipsGprRegisters>(lw.Rt, MipsMachine.DataMemory.ReadWord((ulong)address));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 4,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Sb sb) {
            MipsMachine.DataMemory.WriteByte((ulong)(RegisterBank.Get<MipsGprRegisters>(sb.Rs) + sb.Immediate), (byte)RegisterBank.Get<MipsGprRegisters>(sb.Rt));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)(RegisterBank.Get<MipsGprRegisters>(sb.Rs) + sb.Immediate),
                Size = 1,
                Mode = MemoryAccessMode.Write,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Sh sh) {
            int address = RegisterBank.Get<MipsGprRegisters>(sh.Rs) + sh.Immediate;
            if((address & 0b1) != 0){
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = sh.ConvertToInt(),
                        ProgramCounter = RegisterBank[MipsGprRegisters.Pc]
                    });
                }
                return;
            }
            // write two bytes
            MipsMachine.DataMemory.WriteByte((ulong)address, (byte)(RegisterBank.Get<MipsGprRegisters>(sh.Rt) >> 8));
            MipsMachine.DataMemory.WriteByte((ulong)(address + 1), (byte)(RegisterBank.Get<MipsGprRegisters>(sh.Rt) & 0xFF));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 2,
                Mode = MemoryAccessMode.Write,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Sw sw) {
            int address = RegisterBank.Get<MipsGprRegisters>(sw.Rs) + sw.Immediate;
            if ((address & 0b11) != 0) {
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = sw.ConvertToInt(),
                        ProgramCounter = RegisterBank[MipsGprRegisters.Pc]
                    });
                }
                return;
            }
            MipsMachine.DataMemory.WriteWord((ulong)address, RegisterBank.Get<MipsGprRegisters>(sw.Rt));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 4,
                Mode = MemoryAccessMode.Write,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Teqi teqi) {
            if (RegisterBank.Get<MipsGprRegisters>(teqi.Rs) == teqi.Immediate) {
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.Trap,
                        Instruction = teqi.ConvertToInt(),
                        ProgramCounter = RegisterBank[MipsGprRegisters.Pc]
                    });
                }
            }
        } else if (instruction is Lwcz lwcz)
        {
            ulong address = (ulong)(RegisterBank.Get<MipsGprRegisters>(lwcz.Base) + lwcz.Immediate);
            int value = MipsMachine.DataMemory.ReadWord(address);
            if (lwcz.Coprocessor == 0) { // syscontrol
                RegisterBank.Set<MipsSpecialRegisters>(lwcz.Rt, value);
            }else if (lwcz.Coprocessor == 1) { // fpu
                RegisterBank.Set<MipsFpuRegisters>(lwcz.Rt, value);
            }else {
                if (MipsMachine.StdErr is not null) {
                    await MipsMachine.StdErr.Writer.WriteAsync($"Coprocessor {lwcz.Coprocessor} not supported. Instruction: {lwcz} @ PC={RegisterBank[MipsGprRegisters.Pc]:X8}\n");
                }
                return;
            }
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs()
            {
                Address = address,
                Mode =MemoryAccessMode.Read,
                Size = 4,
                Source = MemoryAccessSource.Instruction
            });
            
        }else if (instruction is Swcz swc1) {
            ulong address = (ulong)(RegisterBank.Get<MipsGprRegisters>(swc1.Base) + swc1.Immediate);
            int value = 0;
            if (swc1.Coprocessor == 0) { // syscontrol
                value = RegisterBank.Get<MipsSpecialRegisters>(swc1.Rt);
            }else if (swc1.Coprocessor == 1) { // fpu
                value = RegisterBank.Get<MipsFpuRegisters>(swc1.Rt);
            }else {
                if (MipsMachine.StdErr is not null) {
                    await MipsMachine.StdErr.Writer.WriteAsync($"Coprocessor {swc1.Coprocessor} not supported. Instruction: {swc1} @ PC={RegisterBank[MipsGprRegisters.Pc]:X8}\n");
                }
                return;
            }
            MipsMachine.DataMemory.WriteWord(address, value);
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs()
            {
                Address = address,
                Mode = MemoryAccessMode.Write,
                Size = 4,
                Source = MemoryAccessSource.Instruction
            });
        } else {
            if (MipsMachine.StdErr is not null) {
                await MipsMachine.StdErr.Writer.WriteAsync($"Type I instruction not implemented: {instruction} @ PC={RegisterBank[MipsGprRegisters.Pc]:X8}\n");
            }
            return;
        } 
    }
}
