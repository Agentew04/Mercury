using System.Threading.Channels;
using ELFSharp.ELF;
using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Runtime;
using SAAE.Engine.Mips.Runtime.OS;
using SAAE.Engine.Mips.Runtime.Simple;

namespace SAAE.Engine.Common;

/// <summary>
/// A class that holds all the parts that the simulated computer needs
/// to function.
/// </summary>
public abstract class Machine : IAsyncClockable, IDisposable
{
    /// <summary>
    /// A reference to the memory object that holds all data
    /// that the program operates on. It may be the same object
    /// as <see cref="InstructionMemory"/>.
    /// </summary>
    public IMemory DataMemory { get; init; } = null!;

    /// <summary>
    /// A reference to the memory object that contains instructions.
    /// It may be the same object as <see cref="DataMemory"/>.
    /// </summary>
    public IMemory InstructionMemory { get; init; } = null!;

    /// <summary>
    /// The object that executes code.
    /// </summary>
    public ICpu Cpu { get; init; }

    /// <summary>
    /// A link to the RegisterBank present on the <see cref="Cpu"/>
    /// </summary>
    public RegisterBank Registers => Cpu.RegisterBank;

    /// <summary>
    /// The Operating System that answers syscalls of this machine.
    /// </summary>
    public required IOperatingSystem Os { get; init; }

    /// <summary>
    /// The standard input that gives data to the program being run.
    /// </summary>
    public required Channel<char> StdIn { get; init; }
    
    /// <summary>
    /// The standard output that programs can write to.
    /// </summary>
    public required Channel<char> StdOut { get; init; }
    
    /// <summary>
    /// The standard channels where errors from the program and machine are written to.
    /// </summary>
    public required Channel<char> StdErr { get; init; }

    /// <summary>
    /// The current architecture of this machine.
    /// </summary>
    public required Architecture Architecture { get; init; }

    public bool IsDisposed { get; private set; }
    
    public async ValueTask ClockAsync() {
        await Cpu.ClockAsync();
        List<(Type, Enum)> dirty = Registers.GetDirty();
        // invoke even with 0 dirty to unselect last changed register on ui
        OnRegisterChanged?.Invoke(dirty);
    }
    
    public bool IsClockingFinished() => Cpu.IsClockingFinished();
    
    /// <summary>
    /// Raised every cycle with a list of the changed registers. Contains
    /// the enum type of the register and the actual register as a base <see cref="Enum"/>.
    /// </summary>
    public event Action<List<(Type,Enum)>>? OnRegisterChanged;
    
    /// <summary>
    /// Event fired when any access to the memory is made.
    /// </summary>
    public event EventHandler<MemoryAccessEventArgs>? MemoryAccessed;

    /// <summary>
    /// Loads a ELF executable into the <see cref="DataMemory"/>.
    /// </summary>
    /// <param name="elf">The ELF file to be loaded</param>
    public abstract void LoadElf(ELF<uint> elf);
    
    protected uint Load(Span<byte> data, uint address) {
        for(ulong i=0;i<(ulong)data.Length; i++) {
            DataMemory.WriteByte(address + i, data[(int)i]);
        }
        return address + (uint)data.Length;
    }
    
    protected uint Load(Span<int> bytes, uint address) {
        for (ulong i = 0; i < (ulong)bytes.Length; i++) {
            DataMemory.WriteWord(address + i*4, bytes[(int)i]);
        }
        return address + (uint)bytes.Length * 4;
    }

    /// <summary>
    /// Loads a program based on a list of words. Separates between instructions and data. 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="data"></param>
    public abstract void LoadProgram(Span<int> text, Span<int> data);
    
    /// <summary>
    /// Disposes all resources used by this machine.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }
        IsDisposed = true;
        
        // remove links
        Cpu.Machine = null!;
        Os.Machine = null!;
        
        // dispose objects
        if(DataMemory is IDisposable dispDMem) dispDMem.Dispose();
        if(InstructionMemory is IDisposable dispIMem) dispIMem.Dispose();
        Os.Dispose();
    }

    /// <summary>
    /// Raises the <see cref="MemoryAccessed"/> event.
    /// </summary>
    /// <param name="e">The arguments of the event</param>
    public void OnMemoryAccess(MemoryAccessEventArgs e)
    {
        MemoryAccessed?.Invoke(this, e);
    }
}