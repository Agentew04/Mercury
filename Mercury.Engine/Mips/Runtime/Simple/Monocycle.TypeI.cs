using Mercury.Engine.Mips.Instructions;
using Mercury.Engine.Common;

namespace Mercury.Engine.Mips.Runtime.Simple;

public partial class Monocycle {

    private async ValueTask ExecuteTypeI(TypeIInstruction instruction) {
        if (instruction is Addi addi) {
            int result = Registers.Get<MipsGprRegisters>(addi.Rs) + addi.Immediate;
            if (IsOverflowed(Registers.Get<MipsGprRegisters>(addi.Rs), addi.Immediate, result)) {
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                        Instruction = addi.ConvertToInt(),
                        ProgramCounter = Registers.Get(MipsGprRegisters.Pc)
                    });
                }
                return;
            }
            Registers.Set<MipsGprRegisters>(addi.Rt, result);
        } else if (instruction is Addiu addiu) {
            Registers.Set<MipsGprRegisters>(addiu.Rt, Registers.Get<MipsGprRegisters>(addiu.Rs) + addiu.Immediate);
        } else if (instruction is Slti slti) {
            Registers.Set<MipsGprRegisters>(slti.Rt, Registers.Get<MipsGprRegisters>(slti.Rs) < slti.Immediate ? 1 : 0);
        } else if (instruction is Sltiu sltiu) {
            Registers.Set<MipsGprRegisters>(sltiu.Rt, (uint)Registers.Get<MipsGprRegisters>(sltiu.Rs) < (ushort)sltiu.Immediate ? 1 : 0);
        } else if (instruction is Beq beq) {
            if (Registers.Get<MipsGprRegisters>(beq.Rs) == Registers.Get<MipsGprRegisters>(beq.Rt)) {
                BranchTo(beq.Immediate);
            }
        } else if (instruction is Bgez bgez) {
            if (Registers.Get<MipsGprRegisters>(bgez.Rs) >= 0) {
                BranchTo(bgez.Immediate);
            }
        } else if (instruction is Bgezal bgezal) {
            if (Registers.Get<MipsGprRegisters>(bgezal.Rs) >= 0) {
                BranchTo(bgezal.Immediate);
                Link();
            }
        } else if(instruction is Bgtz bgtz) {
            if (Registers.Get<MipsGprRegisters>(bgtz.Rs) > 0) {
                BranchTo(bgtz.Immediate);
            }
        } else if(instruction is Blez blez) {
            if (Registers.Get<MipsGprRegisters>(blez.Rs) <= 0) {
                BranchTo(blez.Immediate);
            }
        } else if(instruction is Bltz bltz) {
            if (Registers.Get<MipsGprRegisters>(bltz.Rs) < 0) {
                BranchTo(bltz.Immediate);
            }
        } else if(instruction is Bltzal bltzal) {
            if (Registers.Get<MipsGprRegisters>(bltzal.Rs) < 0) {
                BranchTo(bltzal.Immediate);
                Link();
            }
        } else if(instruction is Bne bne) {
            if (Registers.Get<MipsGprRegisters>(bne.Rs) != Registers.Get<MipsGprRegisters>(bne.Rt)) {
                BranchTo(bne.Immediate);
            }
        } else if(instruction is Andi andi) {
            Registers.Set<MipsGprRegisters>(andi.Rt, Registers.Get<MipsGprRegisters>(andi.Rs) & ZeroExtend(andi.Immediate));
        } else if(instruction is Ori ori) {
            Registers.Set<MipsGprRegisters>(ori.Rt, Registers.Get<MipsGprRegisters>(ori.Rs) | ZeroExtend(ori.Immediate));
        } else if (instruction is Xori xori) {
            Registers.Set<MipsGprRegisters>(xori.Rt, Registers.Get<MipsGprRegisters>(xori.Rs) ^ ZeroExtend(xori.Immediate));
        } else if(instruction is Lb lb) {
            Registers.Set<MipsGprRegisters>(lb.Rt, (sbyte)MipsMachine.DataMemory.ReadByte((ulong)(Registers.Get<MipsGprRegisters>(lb.Rs) + lb.Immediate)));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)(Registers.Get<MipsGprRegisters>(lb.Rs) + lb.Immediate),
                Size = 1,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Lbu lbu) {
            Registers.Set<MipsGprRegisters>(lbu.Rt, MipsMachine.DataMemory.ReadByte((ulong)(Registers.Get<MipsGprRegisters>(lbu.Rs) + lbu.Immediate)));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)(Registers.Get<MipsGprRegisters>(lbu.Rs) + lbu.Immediate),
                Size = 1,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        }else if(instruction is Lh lh) {
            int address = Registers.Get<MipsGprRegisters>(lh.Rs) + lh.Immediate;
            if (address % 2 != 0) {
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = lh.ConvertToInt(),
                        ProgramCounter = Registers[MipsGprRegisters.Pc]
                    });
                }
                return;
            }
            Registers.Set<MipsGprRegisters>(lh.Rt, (short)(MipsMachine.DataMemory.ReadWord((ulong)address) & 0xFFFF));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 2,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Lhu lhu) {
            int address = Registers.Get<MipsGprRegisters>(lhu.Rs) + lhu.Immediate;
            if (address % 2 != 0) {
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = lhu.ConvertToInt(),
                        ProgramCounter = Registers[MipsGprRegisters.Pc]
                    });
                }
                return;
            }
            Registers.Set<MipsGprRegisters>(lhu.Rt, (ushort)(MipsMachine.DataMemory.ReadWord((ulong)address) & 0xFFFF));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 2,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Lui lui) {
            Registers.Set<MipsGprRegisters>(lui.Rt, lui.Immediate << 16);
        } else if(instruction is Lw lw) {
            int address = Registers.Get<MipsGprRegisters>(lw.Rs) + lw.Immediate;
            if((address & 0b11) != 0) {
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = lw.ConvertToInt(),
                        ProgramCounter = Registers[MipsGprRegisters.Pc]
                    });
                }
                return;
            }
            Registers.Set<MipsGprRegisters>(lw.Rt, MipsMachine.DataMemory.ReadWord((ulong)address));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 4,
                Mode = MemoryAccessMode.Read,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Sb sb) {
            MipsMachine.DataMemory.WriteByte((ulong)(Registers.Get<MipsGprRegisters>(sb.Rs) + sb.Immediate), (byte)Registers.Get<MipsGprRegisters>(sb.Rt));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)(Registers.Get<MipsGprRegisters>(sb.Rs) + sb.Immediate),
                Size = 1,
                Mode = MemoryAccessMode.Write,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Sh sh) {
            int address = Registers.Get<MipsGprRegisters>(sh.Rs) + sh.Immediate;
            if((address & 0b1) != 0){
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = sh.ConvertToInt(),
                        ProgramCounter = Registers[MipsGprRegisters.Pc]
                    });
                }
                return;
            }
            // write two bytes
            MipsMachine.DataMemory.WriteByte((ulong)address, (byte)(Registers.Get<MipsGprRegisters>(sh.Rt) >> 8));
            MipsMachine.DataMemory.WriteByte((ulong)(address + 1), (byte)(Registers.Get<MipsGprRegisters>(sh.Rt) & 0xFF));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 2,
                Mode = MemoryAccessMode.Write,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Sw sw) {
            int address = Registers.Get<MipsGprRegisters>(sw.Rs) + sw.Immediate;
            if ((address & 0b11) != 0) {
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = sw.ConvertToInt(),
                        ProgramCounter = Registers[MipsGprRegisters.Pc]
                    });
                }
                return;
            }
            MipsMachine.DataMemory.WriteWord((ulong)address, Registers.Get<MipsGprRegisters>(sw.Rt));
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                Address = (ulong)address,
                Size = 4,
                Mode = MemoryAccessMode.Write,
                Source = MemoryAccessSource.Instruction
            });
        } else if(instruction is Teqi teqi) {
            if (Registers.Get<MipsGprRegisters>(teqi.Rs) == teqi.Immediate) {
                if (SignalException is not null) {
                    await SignalException.Invoke(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.Trap,
                        Instruction = teqi.ConvertToInt(),
                        ProgramCounter = Registers[MipsGprRegisters.Pc]
                    });
                }
            }
        } else if (instruction is Lwcz lwcz)
        {
            ulong address = (ulong)(Registers.Get<MipsGprRegisters>(lwcz.Base) + lwcz.Immediate);
            int value = MipsMachine.DataMemory.ReadWord(address);
            if (lwcz.Coprocessor == 0) { // syscontrol
                Registers.Set<MipsSpecialRegisters>(lwcz.Rt, value);
            }else if (lwcz.Coprocessor == 1) { // fpu
                Registers.Set<MipsFpuRegisters>(lwcz.Rt, value);
            }else {
                await MipsMachine.StdErr.Writer.WriteAsync($"Coprocessor {lwcz.Coprocessor} not supported. Instruction: {lwcz} @ PC={Registers[MipsGprRegisters.Pc]:X8}\n");
                return;
            }
            MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs()
            {
                Address = address,
                Mode =MemoryAccessMode.Read,
                Size = 4,
                Source = MemoryAccessSource.Instruction
            });
            
        }else if (instruction is Swcz swcz) {
            ulong address = (ulong)(Registers.Get<MipsGprRegisters>(swcz.Base) + swcz.Immediate);
            int value;
            if (swcz.Coprocessor == 0) { // syscontrol
                value = Registers.Get<MipsSpecialRegisters>(swcz.Rt);
            }else if (swcz.Coprocessor == 1) { // fpu
                value = Registers.Get<MipsFpuRegisters>(swcz.Rt);
            }else {
                await MipsMachine.StdErr.Writer.WriteAsync($"Coprocessor {swcz.Coprocessor} not supported. Instruction: {swcz} @ PC={Registers[MipsGprRegisters.Pc]:X8}\n");
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
            await MipsMachine.StdErr.Writer.WriteAsync($"Type I instruction not implemented: {instruction} @ PC={Registers[MipsGprRegisters.Pc]:X8}\n");
        } 
    }
}
