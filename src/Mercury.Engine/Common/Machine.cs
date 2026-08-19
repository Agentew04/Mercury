using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;
using Mercury.Engine.Common.Events;
using Mercury.Engine.Memory;
using Mercury.Engine.Mips.Runtime.Simple;

namespace Mercury.Engine.Common;

/// <summary>
/// A class that holds all the parts that the simulated computer needs
/// to function.
/// </summary>
public abstract class Machine : IAsyncClockable, IDisposable {

    public required EventBus EventBus { get; init; } = new();

    public List<IModule> Modules { get; init; } = [];

    /// <summary>
    /// A reference to the memory object that contains instructions.
    /// It may be the same object as <see cref="DataMemory"/>.
    /// </summary>
    public IMemory MemoryModule {
        get {
            field ??= (IMemory?)Modules.Find(x => x is IMemory);
            return field  ?? throw new NullReferenceException("No module implementing IMemory found");
        }
    }

    /// <summary>
    /// The object that executes code.
    /// </summary>
    public ICpuModule CpuModule {
        get {
            field ??= (ICpuModule?)Modules.Find(x => x is ICpuModule);
            return field ?? throw new NullReferenceException("No module found implementing ICpuModule");
        }
    }

    /// <summary>
    /// The Operating System that answers syscalls of this machine.
    /// </summary>
    public ISyscallModule? SyscallModule {
        get {
            field ??= (ISyscallModule?)Modules.Find(x => x is ISyscallModule);
            return field;
        }
    }

    public BufferedStdinModule StdIn {
        get {
            field ??= (BufferedStdinModule?)Modules.Find(x => x is BufferedStdinModule);
            return field ?? throw new NullReferenceException("No BufferedStdinModule found");
        }
    }

    public T? GetModule<T>() where T : IModule => Modules.OfType<T>().FirstOrDefault();

    /// <summary>
    /// The current architecture of this machine.
    /// </summary>
    public required Architecture Architecture { get; init; }

    public bool IsDisposed { get; private set; }
    
    public async ValueTask ClockAsync() {
        await EventBus.PublishAsync(new ClockEvent());
        ValueTuple<Type, int>[] dirty = CpuModule.Registers.GetDirty(out int count);
        OnRegisterChanged?.Invoke(dirty,count);
    }

    public bool IsClockingFinished() => ((Monocycle)Modules.Find(x => x is Monocycle)!).IsClockingFinished();
    
    /// <summary>
    /// Raised every cycle with a list of the changed registers. Contains
    /// the enum type of the register and the actual register as a base <see cref="Enum"/>.
    /// </summary>
    public event Action<ValueTuple<Type,int>[], int>? OnRegisterChanged;

    /// <summary>
    /// Loads a ELF executable into the <see cref="DataMemory"/>.
    /// </summary>
    /// <param name="elf">The ELF file to be loaded</param>
    public virtual void LoadElf(ELF<uint> elf) {
        Section<uint>? textSection = elf.GetSection(".text");
        uint textStart = textSection!.LoadAddress;
        uint textLength = textSection.Size;
        SymbolTable<uint>? symbolTable = elf.GetSections<SymbolTable<uint>>().First();
        CpuModule.ProgramEnd = symbolTable?.Entries?.FirstOrDefault(x => x.Name == "__end")?.Value ?? textStart + textLength;
        CpuModule.Endianess = elf.Endianess == ELFSharp.Endianess.BigEndian
            ? Endianess.BigEndian
            : Endianess.LittleEndian;
        foreach (Segment<uint>? segment in elf.Segments) {
            if (segment.Type != SegmentType.Load) {
                continue;
            }
            MemoryModule.Write(segment.Address, segment.GetMemoryContents());
        }
    }
    
    protected uint Load(Span<byte> data, uint address) {
        for(ulong i=0;i<(ulong)data.Length; i++) {
            MemoryModule.WriteByte(address + i, data[(int)i]);
        }
        return address + (uint)data.Length;
    }
    
    protected uint Load(Span<int> bytes, uint address) {
        for (ulong i = 0; i < (ulong)bytes.Length; i++) {
            MemoryModule.WriteWord(address + i*4, bytes[(int)i]);
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
        
        // dispose objects
        foreach (IModule module in Modules) {
            module.UnsubscribeFromEvents();
            if (module is IDisposable disposable) {
                // dispose on some modules just call UnsubscribeFromEvents()
                // but, theres minimal overhead in ensuring everything
                // is properly disposed on modules that require special
                // procedures.
                disposable.Dispose();
            }
        }
    }
}