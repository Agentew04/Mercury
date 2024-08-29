using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Assembler;
using SAAE.Engine.Mips.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Runtime.Simple;

/// <summary>
/// A simplified version of the monocycle MIPS processor.
/// Does not simulate every component of the processor.
/// </summary>
public partial class Monocycle : IClockable
{
    public Monocycle() {
        // create virtual memory
        const ulong gb = 1024 * 1024 * 1024;
        memory = new VirtualMemory(new VirtualMemoryConfiguration() {
            ColdStoragePath = "memory.bin",
            ColdStorageOptimization = true,
            ForceColdStorageReset = true,
            PageSize = 4096,
            Size = 4*gb,
            MaxLoadedPages = 64
        });
    }

    /// <summary>
    /// Represents the RAM memory
    /// </summary>
    private VirtualMemory memory;

    /// <summary>
    /// Structure that holds all of the general purpose
    /// registers of the CPU.
    /// </summary>
    private RegisterFile registerFile = new();

    private InstructionFactory instructionFactory = new();

    public void Clock()
    {
        // read instruction from PC
        int instructionBinary = memory.ReadWord((ulong)registerFile[RegisterFile.Register.Pc]);

        // decode
        Instruction instruction = instructionFactory.Disassemble(instructionBinary);

        int pcBefore = registerFile[RegisterFile.Register.Pc];
        Execute(instruction);

        // update PC
        if (pcBefore == registerFile[RegisterFile.Register.Pc]) {
            registerFile[RegisterFile.Register.Pc] += 4;
        }
    }

    public event EventHandler<SignalExceptionEventArgs>? OnSignalException = null;

    private void Execute(Instruction instruction) {
        if(instruction is TypeRInstruction r) {
            ExecuteTypeR(r);
        }else if(instruction is TypeIInstruction i) {
            ExecuteTypeI(i);
        }else if(instruction is TypeJInstruction j) {

        }
    }

    private static bool IsOverflowed(int a, int b, int result) {
        return (a > 0 && b > 0 && result < 0) || (a < 0 && b < 0 && result > 0);
    }

    private void BranchTo(int immediate) {
        registerFile[RegisterFile.Register.Pc] += 4 + immediate << 2;
    }
    private void Link(RegisterFile.Register register = RegisterFile.Register.Ra) {
        registerFile[register] = registerFile[RegisterFile.Register.Pc];
    }

    private static int ZeroExtend(short value) {
        return (ushort)value;
    }

    public class SignalExceptionEventArgs {
        public SignalType Signal { get; set; }

        public int ProgramCounter { get; set; }

        public int Instruction { get; set; }

        public enum SignalType {
            Breakpoint,
            SystemCall,
            Trap,
            IntegerOverflow,
            AddressError,
        }
    }
}
