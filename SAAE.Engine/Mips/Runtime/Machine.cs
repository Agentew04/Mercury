using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;
using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Runtime.Simple;

namespace SAAE.Engine.Mips.Runtime;

/// <summary>
/// A class that holds all the parts of the computer needed
/// to function.
/// </summary>
public sealed class Machine : IDisposable, IClockable {

    public VirtualMemory Memory { get; init; } = null!;

    public Monocycle Cpu { get; init; } = null!;

    public RegisterFile Registers => Cpu.RegisterFile;

    public OperatingSystem Os { get; init; } = null!;

    public Stream StdIn { get; init; } = null!;
    
    public Stream StdOut { get; init; } = null!;
    
    public Stream StdErr { get; init; } = null!;
    
    public void LoadElf(ELF<uint> elf) {
        Section<uint>? textSection = elf.GetSection(".text");
        uint textStart = textSection!.LoadAddress;
        uint textLength = textSection.Size;
        Cpu.DropoffAddress = textStart + textLength;
        
        // use segments to load data into memory
        foreach (Segment<uint>? segment in elf.Segments) {
            if (segment.Type != SegmentType.Load) {
                continue;
            }
            Memory.Write(segment.Address, segment.GetMemoryContents());
        }
    }
    
    private const uint TextSegmentAddress = 0x0040_0000;
    private const uint DataSegmentAddress = 0x1001_0000;
    private const uint HeapSegmentAddress = 0x1004_0000;
    private const uint KDataSegmentAddress = 0x9000_0000; 
    private const uint ExternSegmentAddress = 0x1000_0000;
    
    public void LoadProgram(Span<byte> text, Span<byte> data) {
        uint lastText = Load(text, TextSegmentAddress);
        Cpu.DropoffAddress = lastText + 1;
        _ = Load(data, DataSegmentAddress);
    }
    
    public void LoadProgram(Span<int> text, Span<int> data) {
        uint lastText = Load(text, TextSegmentAddress);
        Cpu.DropoffAddress = lastText + 1;
        _ = Load(data, DataSegmentAddress);
    }

    /// <summary>
    /// Load data into memory
    /// </summary>
    /// <param name="data">The data to write</param>
    /// <param name="address">The base address</param>
    /// <returns>Returns the last written address</returns>
    private uint Load(Span<byte> data, uint address) {
        for(ulong i=0;i<(ulong)data.Length; i++) {
            Memory.WriteByte(address + i, data[(int)i]);
        }
        return address + (uint)data.Length;
    }
    
    private uint Load(Span<int> bytes, uint address) {
        for (ulong i = 0; i < (ulong)bytes.Length; i++) {
            Memory.WriteWord(address + i*4, bytes[(int)i]);
        }
        return address + (uint)bytes.Length * 4;
    }

    public void Clock() {
        Cpu.Clock();
        List<RegisterFile.Register> regChanged = Registers.GetChangedRegisters(); // essa chamada reseta a lista 
                                                                                  // p proximo clock
        if(regChanged.Count > 0) {
            OnRegisterChanged?.Invoke(regChanged);
        }
    }
    
    public bool IsClockingFinished() {
        return Cpu.IsClockingFinished();
    }

    public event Action<List<RegisterFile.Register>>? OnRegisterChanged = null;

    public void Dispose() {
        Memory.Dispose();
        Os.Dispose();
        StdIn.Dispose();
        StdOut.Dispose();
        StdErr.Dispose();
    }
}