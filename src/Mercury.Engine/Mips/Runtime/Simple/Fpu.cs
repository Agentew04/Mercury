using System.Numerics;
using System.Runtime.CompilerServices;
using Mercury.Engine.Common;
using Mercury.Engine.Mips.Instructions;
using Mercury.Engine.Mips.Runtime.Events;

namespace Mercury.Engine.Mips.Runtime.Simple;

/// <summary>
/// Floating Point Unit (coprocessor 1) of the MIPS machine. Runs as a module
/// separated from the CPU. It listens to <see cref="UnhandledInstructionEvent"/>,
/// published by the CPU when it does not recognize an instruction, and executes
/// the ones that belong to the FPU. Any other instruction is ignored.
/// </summary>
public sealed class Fpu : IModule {

    private readonly ICpuModule cpu;
    private readonly bool[] flags;
    private readonly List<IDisposable> subscriptions = [];
    private EventBus eventBus = null!;
    private uint currentWord;
    private ulong currentAddress;

    /// <param name="cpu">The CPU that owns the (shared) register file.</param>
    /// <param name="flags">The FPU condition flags (<c>cc</c>).</param>
    public Fpu(ICpuModule cpu, bool[] flags) {
        this.cpu = cpu;
        this.flags = flags;
    }

    private RegisterCollection Registers => cpu.Registers;
    private bool[] Flags => flags;

    public void SubscribeToEvents(EventBus bus) {
        eventBus = bus;
        subscriptions.Add(bus.Subscribe<UnhandledInstructionEvent>(OnUnhandledInstruction));
    }

    public void UnsubscribeFromEvents() {
        foreach (IDisposable subscription in subscriptions) {
            subscription.Dispose();
        }
        subscriptions.Clear();
    }

    private void OnUnhandledInstruction(UnhandledInstructionEvent e) {
        if (e.Handled) {
            return;
        }
        currentWord = e.Word;
        currentAddress = e.Address;
        if (ExecuteTypeF(e.Instruction)) {
            e.Handled = true;
        }
    }

    public void Dispose() {
        UnsubscribeFromEvents();
    }
    private bool ExecuteTypeF(IInstruction instruction) {
        switch (instruction) {
            case Abs abs: {
                switch (abs.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        ExecUnary<float>(abs.Fs,abs.Fd, MathF.Abs);
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        ExecUnary<double>(abs.Fs,abs.Fd, Math.Abs);
                        break;
                    case TypeFInstruction.LongFixedPrecisionFormat:
                    case TypeFInstruction.WordFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case AddFloat add: {
                switch (add.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        ExecBinary<float>(add.Fs, add.Ft, add.Fd, static (a,b)=>a+b);
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        ExecBinary<double>(add.Fs, add.Ft, add.Fd, static (a,b)=>a+b);
                        break;
                    case TypeFInstruction.LongFixedPrecisionFormat:
                    case TypeFInstruction.WordFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case Bc1F bc1F: {
                if (!Flags[bc1F.Cc]) {
                    eventBus.Publish(new BranchRequestEvent { Offset = bc1F.Offset });
                }
                break;
            }
            case Bc1T bc1T: {
                if (Flags[bc1T.Cc]) {
                    eventBus.Publish(new BranchRequestEvent { Offset = bc1T.Offset });
                }
                break;
            }
            case C c: {
                switch (c.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        Flags[c.Cc] = Compare(Read<float>(c.Fs), Read<float>(c.Ft), c.Cond);
                        eventBus.Publish(new FpuFlagsChangedEvent());
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        Flags[c.Cc] = Compare(Read<double>(c.Fs), Read<double>(c.Ft), c.Cond);
                        eventBus.Publish(new FpuFlagsChangedEvent());
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
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
                    case TypeFInstruction.SinglePrecisionFormat:
                        Write<double>(cvtd.Fd, Read<float>(cvtd.Fs));
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        InvalidOp();
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                        // cvt.d.w - convert 32-bit integer to double
                        Write<double>(cvtd.Fd, (double)ReadInt32(cvtd.Fs));
                        break;
                    case TypeFInstruction.LongFixedPrecisionFormat:
                        // cvt.d.l - convert 64-bit integer to double
                        Write<double>(cvtd.Fd, (double)ReadInt64(cvtd.Fs));
                        break;
                }
                break;
            }
            case CvtS cvts: {
                switch (cvts.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        InvalidOp();
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        Write(cvts.Fd, (float)Read<double>(cvts.Fs));
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                        // cvt.s.w - convert 32-bit integer to float
                        Write<float>(cvts.Fd, (float)ReadInt32(cvts.Fs));
                        break;
                    case TypeFInstruction.LongFixedPrecisionFormat:
                        // cvt.s.l - convert 64-bit integer to float
                        Write<float>(cvts.Fd, (float)ReadInt64(cvts.Fs));
                        break;
                }
                break;
            }
            case DivFloat div: {
                switch (div.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        ExecBinary<float>(div.Fs, div.Ft, div.Fd, static (a,b)=>a/b);
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        ExecBinary<double>(div.Fs, div.Ft, div.Fd, static (a,b)=>a/b);
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
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
                    case TypeFInstruction.SinglePrecisionFormat:
                        Registers.Set<MipsFpuRegisters>(mov.Fd, Registers.Get<MipsFpuRegisters>(mov.Fs));
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        Write(mov.Fd, Read<double>(mov.Fs));
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
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
                    case TypeFInstruction.SinglePrecisionFormat:
                        ExecBinary<float>(mul.Fs, mul.Ft, mul.Fd, static (a,b)=>a*b);
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        ExecBinary<double>(mul.Fs, mul.Ft, mul.Fd, static (a,b)=>a*b);
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
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
                    case TypeFInstruction.SinglePrecisionFormat:
                        ExecUnary<float>(sqrt.Fs, sqrt.Fd, MathF.Sqrt);
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        ExecUnary<double>(sqrt.Fs, sqrt.Fd, Math.Sqrt);
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
                        eventBus.Publish(new UnsupportedFormatEvent());
                        break;
                }
                break;
            }
            case SubFloat sub: {
                switch (sub.Format) {
                    case TypeFInstruction.SinglePrecisionFormat:
                        ExecBinary<float>(sub.Fs, sub.Ft, sub.Fd, static (a,b)=>a-b);
                        break;
                    case TypeFInstruction.DoublePrecisionFormat:
                        ExecBinary<double>(sub.Fs, sub.Ft, sub.Fd, static (a,b)=>a-b);
                        break;
                    case TypeFInstruction.WordFixedPrecisionFormat:
                    case TypeFInstruction.LongFixedPrecisionFormat:
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
                Address = currentAddress,
                Word = currentWord
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

        int ReadInt32(int reg) {
            // 32-bit integer in a single FPU register
            return Registers.Get<MipsFpuRegisters>(reg);
        }

        long ReadInt64(int reg) {
            // 64-bit integer in two FPU registers (reg and reg+1)
            // High 32 bits in reg, low 32 bits in reg+1 (big-endian style like double)
            long high = (long)(uint)Registers.Get<MipsFpuRegisters>(reg);
            long low = (long)(uint)Registers.Get<MipsFpuRegisters>(reg + 1);
            return (high << 32) | low;
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

