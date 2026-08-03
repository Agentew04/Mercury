using System.Numerics;
using System.Runtime.CompilerServices;
using Mercury.Engine.Common;
using Mercury.Engine.Mips.Instructions;
using Mercury.Engine.Mips.Runtime.Events;

namespace Mercury.Engine.Mips.Runtime.Simple;

public partial class Monocycle {
    private async ValueTask<bool> ExecuteTypeF(IInstruction instruction) {
        switch (instruction) {
            case Abs abs: {
                switch (abs.Format) {
                    case Instruction.SinglePrecisionFormat:
                        ExecUnary<float>(abs.Fs,abs.Fd, MathF.Abs);
                        break;
                    case Instruction.DoublePrecisionFormat:
                        ExecUnary<double>(abs.Fs,abs.Fd, Math.Abs);
                        break;
                    case Instruction.LongFixedPrecisionFormat:
                    case Instruction.WordFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case AddFloat add: {
                switch (add.Format) {
                    case Instruction.SinglePrecisionFormat:
                        ExecBinary<float>(add.Fs, add.Ft, add.Fd, static (a,b)=>a+b);
                        break;
                    case Instruction.DoublePrecisionFormat:
                        ExecBinary<double>(add.Fs, add.Ft, add.Fd, static (a,b)=>a+b);
                        break;
                    case Instruction.LongFixedPrecisionFormat:
                    case Instruction.WordFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case Bc1F bc1F: {
                if (!Flags[bc1F.Cc]) {
                    BranchTo(bc1F.Offset);
                }
                break;
            }
            case Bc1T bc1T: {
                if (Flags[bc1T.Cc]) {
                    BranchTo(bc1T.Offset);
                }
                break;
            }
            case C c: {
                switch (c.Format) {
                    case Instruction.SinglePrecisionFormat:
                        Flags[c.Cc] = Compare(Read<float>(c.Fs), Read<float>(c.Ft), c.Cond);
                        OnFlagUpdate?.Invoke();
                        break;
                    case Instruction.DoublePrecisionFormat:
                        Flags[c.Cc] = Compare(Read<double>(c.Fs), Read<double>(c.Ft), c.Cond);
                        OnFlagUpdate?.Invoke();
                        break;
                    case Instruction.WordFixedPrecisionFormat:
                    case Instruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case Cfc1 cfc1: {
                Registers.Set<MipsGprRegisters>(cfc1.Rt, Registers.Get<MipsFpuControlRegisters>(cfc1.Fs));
                break;
            }
            case Ctc1 ctc1: {
                Registers.Set<MipsFpuControlRegisters>(ctc1.Fs, Registers.Get<MipsGprRegisters>(ctc1.Rt));
                break;
            }
            case CvtD cvtd: {
                switch (cvtd.Format) {
                    case Instruction.SinglePrecisionFormat:
                        Write<double>(cvtd.Fd, Read<float>(cvtd.Fs));
                        break;
                    case Instruction.DoublePrecisionFormat:
                        InvalidOp();
                        break;
                    case Instruction.WordFixedPrecisionFormat:
                    case Instruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case CvtS cvts: {
                switch (cvts.Format) {
                    case Instruction.SinglePrecisionFormat:
                        InvalidOp();
                        break;
                    case Instruction.DoublePrecisionFormat:
                        Write(cvts.Fd, (float)Read<double>(cvts.Fs));
                        break;
                    case Instruction.WordFixedPrecisionFormat:
                    case Instruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case DivFloat div: {
                switch (div.Format) {
                    case Instruction.SinglePrecisionFormat:
                        ExecBinary<float>(div.Fs, div.Ft, div.Fd, static (a,b)=>a/b);
                        break;
                    case Instruction.DoublePrecisionFormat:
                        ExecBinary<double>(div.Fs, div.Ft, div.Fd, static (a,b)=>a/b);
                        break;
                    case Instruction.WordFixedPrecisionFormat:
                    case Instruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case Mfc1 mfc1: {
                Registers.Set<MipsGprRegisters>(mfc1.Rt, Registers.Get<MipsFpuRegisters>(mfc1.Fs));
                break;
            }
            case Mov mov: {
                switch (mov.Format) {
                    case Instruction.SinglePrecisionFormat:
                        Registers.Set<MipsFpuRegisters>(mov.Fd, Registers.Get<MipsFpuRegisters>(mov.Fs));
                        break;
                    case Instruction.DoublePrecisionFormat:
                        Write(mov.Fd, Read<double>(mov.Fs));
                        break;
                    case Instruction.WordFixedPrecisionFormat:
                    case Instruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case Mtc1 mtc1: {
                Registers.Set<MipsFpuRegisters>(mtc1.Fs, Registers.Get<MipsGprRegisters>(mtc1.Rt));
                break;
            }
            case MulFloat mul: {
                switch (mul.Format) {
                    case Instruction.SinglePrecisionFormat:
                        ExecBinary<float>(mul.Fs, mul.Ft, mul.Fd, static (a,b)=>a*b);
                        break;
                    case Instruction.DoublePrecisionFormat:
                        ExecBinary<double>(mul.Fs, mul.Ft, mul.Fd, static (a,b)=>a*b);
                        break;
                    case Instruction.WordFixedPrecisionFormat:
                    case Instruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case Neg neg: {
                // otimizacao, manipula bit do sinal diretamente
                // nao precisa pensar no double pq o bit do sinal fica no 1o registrador mesmo
                uint val = (uint)Registers.Get<MipsFpuRegisters>(neg.Fs);
                if (val >> 31 > 0)
                {
                    // eh negativo. tem que setar msb para 0
                    val &= 0x7FFF_FFFF;
                }
                else
                {
                    // eh positivo. tem que setar msb para 1
                    val = (val & 0x7FFF_FFFF) | 0x8000_0000;
                }
                Registers.Set<MipsFpuRegisters>(neg.Fd, (int)val);
                break;
            }
            case Sqrt sqrt: {
                switch (sqrt.Format) {
                    case Instruction.SinglePrecisionFormat:
                        ExecUnary<float>(sqrt.Fs, sqrt.Fd, MathF.Sqrt);
                        break;
                    case Instruction.DoublePrecisionFormat:
                        ExecUnary<double>(sqrt.Fs, sqrt.Fd, Math.Sqrt);
                        break;
                    case Instruction.WordFixedPrecisionFormat:
                    case Instruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case SubFloat sub: {
                switch (sub.Format) {
                    case Instruction.SinglePrecisionFormat:
                        ExecBinary<float>(sub.Fs, sub.Ft, sub.Fd, static (a,b)=>a-b);
                        break;
                    case Instruction.DoublePrecisionFormat:
                        ExecBinary<double>(sub.Fs, sub.Ft, sub.Fd, static (a,b)=>a-b);
                        break;
                    case Instruction.WordFixedPrecisionFormat:
                    case Instruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            default:
                return false;
        }

        return true;
       
        void InvalidOp() {
            eventBus.Publish(new InvalidOperationEvent {
                Address = (ulong)Registers.Get(MipsGprRegisters.Pc),
                Word = (uint)BytesToInt32(instructionBuffer.Span)
            });
        }
        
        T Read<T>(int reg) where T : unmanaged {
            if (typeof(T) == typeof(float)) {
                int v = Registers.Get<MipsFpuRegisters>(reg);
                return Unsafe.As<int, T>(ref v);
            }
            if (typeof(T) == typeof(double)) {
                long l =
                    ((long)Registers.Get<MipsFpuRegisters>(reg) << 32) |
                    (uint)Registers.Get<MipsFpuRegisters>(reg + 1);
                return Unsafe.As<long, T>(ref l);
            }

            throw new NotSupportedException(
                "This should not happen. Mercury::Engine::Mips::Runtime::Simple::Monocycle.TypeF::ReadFloating<T> " +
                "called with invalid type parameter");
        }

        void Write<T>(int reg, T value) where T : unmanaged {
            if (typeof(T) == typeof(float))
            {
                int bits = Unsafe.As<T, int>(ref value);

                Registers.Set<MipsFpuRegisters>(reg, bits);
                return;
            }

            if (typeof(T) == typeof(double))
            {
                long bits = Unsafe.As<T, long>(ref value);

                Registers.Set<MipsFpuRegisters>(
                    reg, (int)((bits >> 32) & 0xFFFF_FFFF));

                Registers.Set<MipsFpuRegisters>(
                    reg + 1, (int)(bits & 0xFFFF_FFFF));

                return;
            }

            throw new NotSupportedException(
                "This should not happen. Mercury::Engine::Mips::Runtime::Simple::Monocycle.TypeF::WriteFloating<T> " +
                "called with invalid type parameter");
        }

        void ExecUnary<T>(int fs, int fd, Func<T, T> op) where T: unmanaged {
            T v = Read<T>(fs);
            T r = op(v);
            Write(fd, r);
        }
        
        void ExecBinary<T>(int fs, int ft, int fd, Func<T, T, T> op)
            where T : unmanaged
        {
            T a = Read<T>(fs);
            T b = Read<T>(ft);
            T r = op(a, b);
            Write(fd, r);
        }
        
        bool Compare<T>(T a, T b, byte cond) where T : IEquatable<T>, IFloatingPoint<T> {
            switch (cond) {
                case 0:
                    return false; // false
                // unordered
                case 1:
                    return T.IsNaN(a) || T.IsNaN(b);
                // equal
                case 2:
                    return a == b;
                // unordered or equal
                case 3:
                    return T.IsNaN(a) || T.IsNaN(b) || a == b;
                //ordered or less than
                case 4:
                    return !T.IsNaN(a) && !T.IsNaN(b) && a < b;
                // unordered or less than
                case 5:
                    return T.IsNaN(a) || T.IsNaN(b) || a < b;
                // ordered or less than or equal
                case 6:
                    return a <= b && !T.IsNaN(a) && !T.IsNaN(b);
                // unordered or less than or equal
                case 7:
                    return T.IsNaN(a) || T.IsNaN(b) || a <= b;
                // signaling false
                case 8: {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        InvalidOp();
                    }
                    return false;
                }
                // not greater than or less than or equal
                case 9:
                    return !(a > b || a < b || a == b) ||
                           T.IsNaN(a) || T.IsNaN(b);
                // signaling equal
                case 10: {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        InvalidOp();
                    }

                    return a == b && !T.IsNaN(a) && !T.IsNaN(b);
                }
                // not greater than or less than
                case 11: {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        InvalidOp();
                    }
                    return !(a > b || a < b) ||
                           T.IsNaN(a) || T.IsNaN(b);
                }
                // less than
                case 12: {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        InvalidOp();
                    }

                    return a < b && !T.IsNaN(a) && !T.IsNaN(b);
                }
                //not greater than or equal
                case 13: {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        InvalidOp();
                    }
                    return !(a >= b) ||
                           T.IsNaN(a) || T.IsNaN(b);
                }
                // less than or equal
                case 14: {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        InvalidOp();
                    }
                    return a <= b && !T.IsNaN(a) && !T.IsNaN(b);
                }
                // not greater than
                case 15: {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        InvalidOp();
                    }
                    return !(a > b) ||
                           T.IsNaN(a) || T.IsNaN(b);
                }
                default:
                    eventBus.Publish(new UnsupportedFormatEvent());
                    return false;
            }
        }
    }
    
    
        
}