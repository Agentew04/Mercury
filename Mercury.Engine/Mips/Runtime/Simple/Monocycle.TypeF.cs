using System.Numerics;
using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Mips.Runtime.Simple;

public partial class Monocycle
{
    private async ValueTask ExecuteTypeF(TypeFInstruction instruction)
    {
        if (instruction is Abs abs)
        {
            if (abs.IsDouble)
            {
                double fs = ReadDouble(abs.Fs);
                double fd = Math.Abs(fs);
                WriteDouble(abs.Fd, fd);
            }
            else
            {
                float fs = ReadFloat(abs.Fs);
                float fd = MathF.Abs(fs);
                WriteFloat(abs.Fd, fd);
            }
        }
        else if (instruction is Add_float add)
        {
            if (add.IsDouble)
            {
                double fs = ReadDouble(add.Fs);
                double ft = ReadDouble(add.Ft);
                double fd = fs + ft;
                WriteDouble(add.Fd, fd);
            }
            else
            {
                float fs = ReadFloat(add.Fs);
                float ft = ReadFloat(add.Ft);
                float fd = fs + ft;
                WriteFloat(add.Fd, fd);
            }
        }
        else if (instruction is Bc1f bc1f)
        {
            if (!Flags[bc1f.Cc])
            {
                BranchTo(bc1f.Offset);
            }
        }
        else if (instruction is Bc1t bc1t)
        {
            if (Flags[bc1t.Cc])
            {
                BranchTo(bc1t.Offset);
            }
        }
        else if (instruction is C c)
        {
            
            async ValueTask<bool> Compare<T>(T a, T b, byte cond) where T : IEquatable<T>, IFloatingPoint<T>
            {
                if (cond == 0) return false; // false
                if (cond == 1) // unordered
                {
                    return T.IsNaN(a) || T.IsNaN(b);
                }
                if (cond == 2) // equal
                {
                    return a == b;
                }
                if (cond == 3) // unordered or equal
                {
                    return T.IsNaN(a) || T.IsNaN(b) || a == b;
                }
                if (cond == 4) //ordered or less than
                {
                    return !T.IsNaN(a) && !T.IsNaN(b) && a < b;
                }
                if (cond == 5) // unordered or less than
                {
                    return T.IsNaN(a) || T.IsNaN(b) || a < b;
                }
                if (cond == 6) // ordered or less than or equal
                {
                    return a <= b && !T.IsNaN(a) && !T.IsNaN(b);
                }
                if (cond == 7) // unordered or less than or equal
                {
                    return T.IsNaN(a) || T.IsNaN(b) || a <= b;
                }

                if (cond == 8) // signaling false
                {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        await InvalidOp();
                    }
                    return false;
                }

                if (cond == 9) // not greater than or less than or equal
                {
                    return !(a > b || a < b || a == b) ||
                           T.IsNaN(a) || T.IsNaN(b);
                }
                if (cond == 10) // signaling equal
                {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        await InvalidOp();
                    }

                    return a == b && !T.IsNaN(a) && !T.IsNaN(b);
                }

                if (cond == 11) // not greater than or less than
                {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        await InvalidOp();
                    }
                    return !(a > b || a < b) ||
                           T.IsNaN(a) || T.IsNaN(b);
                }

                if (cond == 12) // less than
                {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        await InvalidOp();
                    }

                    return a < b && !T.IsNaN(a) && !T.IsNaN(b);
                }

                if (cond == 13) //not greater than or equal
                {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        await InvalidOp();
                    }
                    return !(a >= b) ||
                           T.IsNaN(a) || T.IsNaN(b);
                }

                if (cond == 14) // less than or equal
                {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        await InvalidOp();
                    }
                    return a <= b && !T.IsNaN(a) && !T.IsNaN(b);
                }
                if (cond == 15) // not greater than
                {
                    if (T.IsNaN(a) || T.IsNaN(b))
                    {
                        await InvalidOp();
                    }
                    return !(a > b) ||
                           T.IsNaN(a) || T.IsNaN(b);
                }

                Console.WriteLine("Invalid condition code: " + cond);
                return false;
            }
            
            if (c.IsDouble)
            {
                double fs = ReadDouble(c.Fs);
                double ft = ReadDouble(c.Ft);
                Flags[c.Cc] = await Compare(fs, ft, c.Cond);
            }
            else
            {
                float fs = ReadFloat(c.Fs);
                float ft = ReadFloat(c.Ft);
                Flags[c.Cc] = await Compare(fs, ft, c.Cond);
            }
        }
        else if (instruction is Cfc1 cfc1)
        {
            Registers.Set<MipsGprRegisters>(cfc1.Rt, Registers.Get<MipsFpuControlRegisters>(cfc1.Fs));
        }
        else if (instruction is Ctc1 ctc1)
        {
            Registers.Set<MipsFpuControlRegisters>(ctc1.Fs, Registers.Get<MipsGprRegisters>(ctc1.Rt));
        }
        else if (instruction is Cvt_D ctvd)
        {
            if (ctvd.Fmt == TypeFInstruction.SinglePrecisionFormat)
            {
                float fs = ReadFloat(ctvd.Fs);
                WriteDouble(ctvd.Fd, fs);
            }
        }
        else if (instruction is Cvt_S cvts)
        {
            if (cvts.Fmt == TypeFInstruction.DoublePrecisionFormat)
            {
                double fs = ReadDouble(cvts.Fs);
                WriteFloat(cvts.Fd, (float)fs);
            }
        }
        else if (instruction is Div_float div)
        {
            if (div.IsDouble)
            {
                double fs = ReadDouble(div.Fs);
                double ft = ReadDouble(div.Ft);
                double fd = fs / ft;
                WriteDouble(div.Fd, fd);
            }
            else
            {
                float fs = ReadFloat(div.Fs);
                float ft = ReadFloat(div.Ft);
                float fd = fs / ft;
                WriteFloat(div.Fd, fd);
            }
        }
        else if (instruction is Mfc1 mfc1)
        {
            Registers.Set<MipsGprRegisters>(mfc1.Rt, Registers.Get<MipsFpuRegisters>(mfc1.Fs));
        }else if (instruction is Mov mov)
        {
            if (mov.IsDouble)
            {
                double fs = ReadDouble(mov.Fs);
                WriteDouble(mov.Fd, fs);
            }
            else
            {
                Registers.Set<MipsFpuRegisters>(mov.Fd, Registers.Get<MipsFpuRegisters>(mov.Fs));
            }
        }else if (instruction is Mtc1 mtc1)
        {
            Registers.Set<MipsFpuRegisters>(mtc1.Fs, Registers.Get<MipsGprRegisters>(mtc1.Rt));
        }else if (instruction is Mul_float mul)
        {
            if (mul.IsDouble)
            {
                double fs = ReadDouble(mul.Fs);
                double ft = ReadDouble(mul.Ft);
                double fd = fs * ft;
                WriteDouble(mul.Fd, fd);
            }
            else
            {
                float fs = ReadFloat(mul.Fs);
                float ft = ReadFloat(mul.Ft);
                float fd = fs * ft;
                WriteFloat(mul.Fd, fd);
            }
        }else if (instruction is Neg neg)
        {
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
        }else if (instruction is Sqrt sqrt)
        {
            if (sqrt.IsDouble)
            {
                double fs = ReadDouble(sqrt.Fs);
                double fd = Math.Sqrt(fs);
                WriteDouble(sqrt.Fd, fd);
            }
            else
            {
                float fs = ReadFloat(sqrt.Fs);
                float fd = MathF.Sqrt(fs);
                WriteFloat(sqrt.Fd, fd);
            }
        }else if (instruction is Sub_float sub)
        {
            if (sub.IsDouble)
            {
                double fs = ReadDouble(sub.Fs);
                double ft = ReadDouble(sub.Ft);
                double fd = fs - ft;
                WriteDouble(sub.Fd, fd);
            }
            else
            {
                float fs = ReadFloat(sub.Fs);
                float ft = ReadFloat(sub.Ft);
                float fd = fs - ft;
                WriteFloat(sub.Fd, fd);
            }
        }

        return;

        Task InvalidOp()
        {
            if (SignalException is not null)
            {
                return SignalException.Invoke(new SignalExceptionEventArgs
                {
                    Instruction = MipsMachine.Memory.ReadWord((ulong)Registers.Get(MipsGprRegisters.Pc)),
                    ProgramCounter = Registers.Get(MipsGprRegisters.Pc),
                    Signal = SignalExceptionEventArgs.SignalType.InvalidOperation
                });
            }
            return Task.CompletedTask;
        }
        
        double ReadDouble(int reg)
        {
            long l = ((long)Registers.Get<MipsFpuRegisters>(reg) << 32)
                     | (uint)Registers.Get<MipsFpuRegisters>(reg+1);
            return BitConverter.Int64BitsToDouble(l);
        }
        
        void WriteDouble(int reg, double value)
        {
            long res = BitConverter.DoubleToInt64Bits(value);
            Registers.Set<MipsFpuRegisters>(
                reg, (int)((res >> 32) & 0xFFFF_FFFF));
            Registers.Set<MipsFpuRegisters>(
                reg+1, (int)(res & 0xFFFF_FFFF));
        }
        
        float ReadFloat(int reg)
        {
            return BitConverter.Int32BitsToSingle(Registers.Get<MipsFpuRegisters>(reg));
        }
        
        void WriteFloat(int reg, float value)
        {
            Registers.Set<MipsFpuRegisters>(
                reg, BitConverter.SingleToInt32Bits(value));
        }
    }
        
}