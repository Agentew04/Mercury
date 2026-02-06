using Mercury.Engine.Mips.Instructions;
using Mercury.Engine.Common;

namespace Mercury.Engine.Mips.Runtime.Simple;

public partial class Monocycle {

    private async ValueTask<bool> ExecuteTypeI(IInstruction instruction) {
        switch (instruction) {
            case Addi addi: {
                int result = Registers.Get<MipsGprRegisters>(addi.Rs) + addi.Immediate;
                if (IsOverflowed(Registers.Get<MipsGprRegisters>(addi.Rs), addi.Immediate, result)) {
                    await InvokeSignal(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.IntegerOverflow,
                        Instruction = (int)addi.ConvertToInt(),
                        ProgramCounter = Registers.Get(MipsGprRegisters.Pc)
                    });
                    break;
                }
                Registers.Set<MipsGprRegisters>(addi.Rt, result);
                break;
            }
            case Addiu addiu: {
                Registers.Set<MipsGprRegisters>(addiu.Rt, Registers.Get<MipsGprRegisters>(addiu.Rs) + addiu.Immediate);
                break;
            }
            case Slti slti: {
                Registers.Set<MipsGprRegisters>(slti.Rt, Registers.Get<MipsGprRegisters>(slti.Rs) < slti.Immediate ? 1 : 0);
                break;
            }
            case Sltiu sltiu: {
                Registers.Set<MipsGprRegisters>(sltiu.Rt, (uint)Registers.Get<MipsGprRegisters>(sltiu.Rs) < (ushort)sltiu.Immediate ? 1 : 0);
                break;
            }
            case Beq beq: {
                if (Registers.Get<MipsGprRegisters>(beq.Rs) == Registers.Get<MipsGprRegisters>(beq.Rt)) {
                    BranchTo(beq.Offset);
                }
                break;
            }
            case Bgez bgez: {
                if (Registers.Get<MipsGprRegisters>(bgez.Rs) >= 0) {
                    BranchTo(bgez.Offset);
                }
                break;
            }
            case Bgezal bgezal: {
                if (Registers.Get<MipsGprRegisters>(bgezal.Rs) >= 0) {
                    BranchTo(bgezal.Immediate);
                    Link();
                }
                break;
            }
            case Bgtz bgtz: {
                if (Registers.Get<MipsGprRegisters>(bgtz.Rs) > 0) {
                    BranchTo(bgtz.Immediate);
                }
                break;
            }
            case Blez blez: {
                if (Registers.Get<MipsGprRegisters>(blez.Rs) <= 0) {
                    BranchTo(blez.Offset);
                }
                break;
            }
            case Bltz bltz: {
                if (Registers.Get<MipsGprRegisters>(bltz.Rs) < 0) {
                    BranchTo(bltz.Offset);
                }
                break;
            }
            case Bltzal bltzal: {
                if (Registers.Get<MipsGprRegisters>(bltzal.Rs) < 0) {
                    BranchTo(bltzal.Offset);
                    Link();
                }
                break;
            }
            case Bne bne: {
                if (Registers.Get<MipsGprRegisters>(bne.Rs) != Registers.Get<MipsGprRegisters>(bne.Rt)) {
                    BranchTo(bne.Offset);
                }
                break;
            }
            case Andi andi: {
                Registers.Set<MipsGprRegisters>(andi.Rt, Registers.Get<MipsGprRegisters>(andi.Rs) & ZeroExtend(andi.Immediate));
                break;
            }
            case Ori ori: {
                Registers.Set<MipsGprRegisters>(ori.Rt, Registers.Get<MipsGprRegisters>(ori.Rs) | ZeroExtend(ori.Immediate));
                break;
            }
            case Xori xori: {
                Registers.Set<MipsGprRegisters>(xori.Rt, Registers.Get<MipsGprRegisters>(xori.Rs) ^ ZeroExtend(xori.Immediate));
                break;
            }
            case Lb lb: {
                ulong address = (ulong)(Registers.Get<MipsGprRegisters>(lb.Base) + lb.Offset);
                Registers.Set<MipsGprRegisters>(lb.Rt, (sbyte)MipsMachine.DataMemory.ReadByte(address));
                MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs {
                    Address = (ulong)(Registers.Get<MipsGprRegisters>(lb.Base) + lb.Offset),
                    Size = 1,
                    Mode = MemoryAccessMode.Read,
                    Source = MemoryAccessSource.Instruction
                });
                break;
            }
            case Lbu lbu: {
                ulong address = (ulong)(Registers.Get<MipsGprRegisters>(lbu.Base) + lbu.Offset);
                Registers.Set<MipsGprRegisters>(lbu.Rt, MipsMachine.DataMemory.ReadByte(address));
                MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                    Address = address,
                    Size = 1,
                    Mode = MemoryAccessMode.Read,
                    Source = MemoryAccessSource.Instruction
                });
                break;
            }
            case Lh lh: {
                int address = Registers.Get<MipsGprRegisters>(lh.Base) + lh.Offset;
                if (address % 2 != 0) {
                    await InvokeSignal(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = (int)lh.ConvertToInt(),
                        ProgramCounter = Registers.Get(MipsGprRegisters.Pc)
                    });
                    break;
                }
                Registers.Set<MipsGprRegisters>(lh.Rt, (short)(MipsMachine.DataMemory.ReadWord((ulong)address) & 0xFFFF));
                MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                    Address = (ulong)address,
                    Size = 2,
                    Mode = MemoryAccessMode.Read,
                    Source = MemoryAccessSource.Instruction
                });
                break;
            }
            case Lhu lhu: {
                int address = Registers.Get<MipsGprRegisters>(lhu.Base) + lhu.Offset;
                if (address % 2 != 0) {
                    await InvokeSignal(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = (int)lhu.ConvertToInt(),
                        ProgramCounter = Registers.Get(MipsGprRegisters.Pc)
                    });
                    break;
                }
                Registers.Set<MipsGprRegisters>(lhu.Rt, (ushort)(MipsMachine.DataMemory.ReadWord((ulong)address) & 0xFFFF));
                MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                    Address = (ulong)address,
                    Size = 2,
                    Mode = MemoryAccessMode.Read,
                    Source = MemoryAccessSource.Instruction
                });
                break;
            }
            case Lui lui: {
                Registers.Set<MipsGprRegisters>(lui.Rt, lui.Immediate << 16);
                break;
            }
            case Lw lw: {
                int address = Registers.Get<MipsGprRegisters>(lw.Base) + lw.Offset;
                if((address & 0b11) != 0) {
                    await InvokeSignal(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = (int)lw.ConvertToInt(),
                        ProgramCounter = Registers.Get(MipsGprRegisters.Pc)
                    });
                    break;
                }
                Registers.Set<MipsGprRegisters>(lw.Rt, MipsMachine.DataMemory.ReadWord((ulong)address));
                MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                    Address = (ulong)address,
                    Size = 4,
                    Mode = MemoryAccessMode.Read,
                    Source = MemoryAccessSource.Instruction
                });
                break;
            }
            case Sb sb: {
                MipsMachine.DataMemory.WriteByte((ulong)(Registers.Get<MipsGprRegisters>(sb.Base) + sb.Offset), (byte)Registers.Get<MipsGprRegisters>(sb.Rt));
                MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                    Address = (ulong)(Registers.Get<MipsGprRegisters>(sb.Base) + sb.Offset),
                    Size = 1,
                    Mode = MemoryAccessMode.Write,
                    Source = MemoryAccessSource.Instruction
                });
                break;
            }
            case Sh sh: {
                int address = Registers.Get<MipsGprRegisters>(sh.Base) + sh.Offset;
                if((address & 0b1) != 0) {
                    await InvokeSignal(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = (int)sh.ConvertToInt(),
                        ProgramCounter = Registers.Get(MipsGprRegisters.Pc)
                    });
                    break;
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
                break;
            }
            case Sw sw: {
                int address = Registers.Get<MipsGprRegisters>(sw.Base) + sw.Offset;
                if ((address & 0b11) != 0) {
                    await InvokeSignal(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.AddressError,
                        Instruction = (int)sw.ConvertToInt(),
                        ProgramCounter = Registers.Get(MipsGprRegisters.Pc)
                    });
                    break;
                }
                MipsMachine.DataMemory.WriteWord((ulong)address, Registers.Get<MipsGprRegisters>(sw.Rt));
                MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs() {
                    Address = (ulong)address,
                    Size = 4,
                    Mode = MemoryAccessMode.Write,
                    Source = MemoryAccessSource.Instruction
                });
                break;
            }
            case Teqi teqi: {
                if (Registers.Get<MipsGprRegisters>(teqi.Rs) == teqi.Immediate) {
                    await InvokeSignal(new SignalExceptionEventArgs() {
                        Signal = SignalExceptionEventArgs.SignalType.Trap,
                        Instruction = (int)teqi.ConvertToInt(),
                        ProgramCounter = Registers.Get(MipsGprRegisters.Pc)
                    });
                }
                break;
            }
            case Lwcz lwcz: {
                ulong address = (ulong)(Registers.Get<MipsGprRegisters>(lwcz.Base) + lwcz.Offset);
                int value = MipsMachine.DataMemory.ReadWord(address);
                if (lwcz.Coprocessor == 0) { // syscontrol
                    Registers.Set<MipsSpecialRegisters>(lwcz.Ft, value);
                }else if (lwcz.Coprocessor == 1) { // fpu
                    Registers.Set<MipsFpuRegisters>(lwcz.Ft, value);
                }else {
                    await MipsMachine.StdErr.Writer.WriteAsync($"Coprocessor {lwcz.Coprocessor} not supported. Instruction: {lwcz} @ PC={Registers.Get(MipsGprRegisters.Pc):X8}\n");
                    break;
                }
                MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs()
                {
                    Address = address,
                    Mode =MemoryAccessMode.Read,
                    Size = 4,
                    Source = MemoryAccessSource.Instruction
                });
                break;
            }
            case Swcz swcz: {
                ulong address = (ulong)(Registers.Get<MipsGprRegisters>(swcz.Base) + swcz.Offset);
                int value;
                if (swcz.Coprocessor == 0) { // syscontrol
                    value = Registers.Get<MipsSpecialRegisters>(swcz.Rt);
                }else if (swcz.Coprocessor == 1) { // fpu
                    value = Registers.Get<MipsFpuRegisters>(swcz.Rt);
                }else {
                    await MipsMachine.StdErr.Writer.WriteAsync($"Coprocessor {swcz.Coprocessor} not supported. Instruction: {swcz} @ PC={Registers.Get(MipsGprRegisters.Pc):X8}\n");
                    break;
                }
                MipsMachine.DataMemory.WriteWord(address, value);
                MipsMachine.OnMemoryAccess(new MemoryAccessEventArgs()
                {
                    Address = address,
                    Mode = MemoryAccessMode.Write,
                    Size = 4,
                    Source = MemoryAccessSource.Instruction
                });
                break;
            }
            default:
                return false;
        }
        return true;
    }
}
