using Mercury.Engine.Mips.Instructions;
using Mercury.Engine.Common;
using Mercury.Engine.Mips.Runtime.Events;

namespace Mercury.Engine.Mips.Runtime.Simple;

public partial class Monocycle {

    private Memory<byte> memoryBuffer = new byte[4];
    
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
                ulong address = (ulong)((uint)Registers.Get<MipsGprRegisters>(lb.Base) + lb.Offset);
                ReadMemory(address, memoryBuffer[..1]);
                Registers.Set<MipsGprRegisters>(lb.Rt, (sbyte)memoryBuffer.Span[0]);
                break;
            }
            case Lbu lbu: {
                ulong address = (ulong)((uint)Registers.Get<MipsGprRegisters>(lbu.Base) + lbu.Offset);
                ReadMemory(address, memoryBuffer[..1]);
                Registers.Set<MipsGprRegisters>(lbu.Rt, memoryBuffer.Span[0]);
                break;
            }
            case Lhu lhu: {
                ulong address = (ulong)((uint)Registers.Get<MipsGprRegisters>(lhu.Base) + lhu.Offset);
                if (address % 2 != 0) {
                    eventBus.Publish(new UnalignedMemoryAccessEvent {
                        InstructionWord = lhu.ConvertToInt(),
                        AccessSize = 2,
                        InstructionAddress = (ulong)Registers.Get(MipsGprRegisters.Pc),
                        MemoryAddress = address
                    });
                    break;
                }
                ReadMemory(address, memoryBuffer[..2]);
                ushort value = (ushort)BytesToInt16(memoryBuffer.Span[..2]);
                Registers.Set<MipsGprRegisters>(lhu.Rt, value);
                break;
            }
            case Lw lw: {
                ulong address = (ulong)((uint)Registers.Get<MipsGprRegisters>(lw.Base) + lw.Offset);
                if((address & 0b11) != 0) {
                    eventBus.Publish(new UnalignedMemoryAccessEvent {
                        InstructionWord =  lw.ConvertToInt(),
                        AccessSize = 4,
                        InstructionAddress = (ulong)Registers.Get(MipsGprRegisters.Pc),
                        MemoryAddress = address
                    });
                    break;
                }
                ReadMemory(address, memoryBuffer);
                int value = BytesToInt32(memoryBuffer.Span);
                Registers.Set<MipsGprRegisters>(lw.Rt, value);
                break;
            }
            case Sb sb: {
                ulong address = (ulong)((uint)Registers.Get<MipsGprRegisters>(sb.Base) + sb.Offset);
                byte value = (byte)(Registers.Get<MipsGprRegisters>(sb.Rt) & 0xFF);
                memoryBuffer.Span[0] = value;
                WriteMemory(address, memoryBuffer[..1]);
                break;
            }
            case Sw sw: {
                ulong address = (ulong)((uint)Registers.Get<MipsGprRegisters>(sw.Base) + sw.Offset);
                if ((address & 0b11) != 0) {
                    eventBus.Publish(new UnalignedMemoryAccessEvent {
                        InstructionWord = sw.ConvertToInt(),
                        AccessSize = 2,
                        InstructionAddress = (ulong)Registers.Get(MipsGprRegisters.Pc),
                        MemoryAddress = address
                    });
                    break;
                }
                int value = Registers.Get<MipsGprRegisters>(sw.Rt);
                Int32ToBytes(value, memoryBuffer.Span);
                WriteMemory(address, memoryBuffer);
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
                ulong address = (ulong)((uint)Registers.Get<MipsGprRegisters>(lwcz.Base) + lwcz.Offset);
                ReadMemory(address, memoryBuffer);
                int value = BytesToInt32(memoryBuffer.Span);
                if (lwcz.Coprocessor == 0) { // syscontrol
                    Registers.Set<MipsSpecialRegisters>(lwcz.Ft, value);
                }else if (lwcz.Coprocessor == 1) { // fpu
                    Registers.Set<MipsFpuRegisters>(lwcz.Ft, value);
                }else {
                    eventBus.Publish(new UnsupportedCoprocessorEvent() {
                        Address = (uint)Registers.Get(MipsGprRegisters.Pc),
                        Instruction = lwcz.ConvertToInt(),
                        Value = lwcz.Coprocessor
                    });
                }
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
                    eventBus.Publish(new UnsupportedCoprocessorEvent() {
                        Address = (uint)Registers.Get(MipsGprRegisters.Pc),
                        Instruction = swcz.ConvertToInt(),
                        Value = swcz.Coprocessor
                    });
                    break;
                }
                Int32ToBytes(value, memoryBuffer.Span);
                WriteMemory(address, memoryBuffer);
                break;
            }
            default:
                return false;
        }
        return true;
    }
}
