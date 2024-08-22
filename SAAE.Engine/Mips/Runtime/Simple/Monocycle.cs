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
public class Monocycle : IClockable
{
    public Monocycle() {
        // create virtual memory
        ulong gb = 1024 * 1024 * 1024;
        memory = new VirtualMemory(new VirtualMemoryConfiguration() {
            ColdStoragePath = "memory.bin",
            ColdStorageOptimization = true,
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

        Execute(instruction);

        // update PC
    }

    private void Execute(Instruction instruction) {

    }
}
