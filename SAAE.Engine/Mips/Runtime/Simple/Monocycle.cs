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
public sealed partial class Monocycle : IClockable, IDisposable {
    public Monocycle() {
        // create virtual memory
        const ulong gb = 1024 * 1024 * 1024;
        Memory = new VirtualMemory(new VirtualMemoryConfiguration() {
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
    public VirtualMemory Memory { get; private set; }

    /// <summary>
    /// Structure that holds all of the general purpose
    /// registers of the CPU.
    /// </summary>
    public RegisterFile RegisterFile { get; private set; } = new();

    /// <summary>
    /// Class responsible to interpret binary instructions
    /// and return the corresponding class.
    /// </summary>
    private readonly InstructionFactory instructionFactory = new();

    public bool UseBranchDelaySlot { get; set; } = false;

    private uint lastAvailablePcAddress = 0;

    public void Clock()
    {
        // read instruction from PC
        int instructionBinary = Memory.ReadWord((ulong)RegisterFile[RegisterFile.Register.Pc]);

        // decode
        Instruction instruction = instructionFactory.Disassemble(instructionBinary);

        Console.WriteLine($"Executing: {instruction.GetType().Name}");
        int pcBefore = RegisterFile[RegisterFile.Register.Pc];
        Execute(instruction);

        // update PC
        if (pcBefore == RegisterFile[RegisterFile.Register.Pc]) {
            RegisterFile[RegisterFile.Register.Pc] += 4;
        }
    }

    public event EventHandler<SignalExceptionEventArgs>? OnSignalException = null;

    private void Execute(Instruction instruction) {
        if(instruction is TypeRInstruction r) {
            ExecuteTypeR(r);
        }else if(instruction is TypeIInstruction i) {
            ExecuteTypeI(i);
        }else if(instruction is TypeJInstruction j) {
            ExecuteTypeJ(j);
        }
    }

    private static bool IsOverflowed(int a, int b, int result) {
        return (a > 0 && b > 0 && result < 0) || (a < 0 && b < 0 && result > 0);
    }

    private void BranchTo(int immediate) {
        RegisterFile[RegisterFile.Register.Pc] += 4 + immediate << 2;
    }
    private void Link(RegisterFile.Register register = RegisterFile.Register.Ra) {
        RegisterFile[register] = RegisterFile[RegisterFile.Register.Pc];
    }

    private static int ZeroExtend(short value) {
        return (ushort)value;
    }

    public void Dispose() {
        Memory.Dispose();
    }

    /// <summary>
    /// Loads data into the Text Section of RAM
    /// </summary>
    /// <param name="data">The byte oriented data</param>
    public void LoadTextSection(Span<byte> data) {
        uint textAddress = 0x0040_0000;
        for(ulong i=0;i<(ulong)data.Length; i++) {
            Memory.WriteByte(textAddress + i, data[(int)i]);
        }
        lastAvailablePcAddress = textAddress + (uint)data.Length + 1;
    }

    /// <summary>
    /// Loads a series of words/instructions into the RAM
    /// </summary>
    /// <param name="data"></param>
    public void LoadTextSection(Span<int> data) {
        uint textAddress = 0x0040_0000;
        for (ulong i = 0; i < (ulong)data.Length; i++) {
            Memory.WriteWord(textAddress + i*4, data[(int)i]);
        }
        lastAvailablePcAddress = textAddress + (uint)data.Length*4 + 1;
    }

    public bool IsExecutionFinished() {
        return RegisterFile[RegisterFile.Register.Pc] <= lastAvailablePcAddress;
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
