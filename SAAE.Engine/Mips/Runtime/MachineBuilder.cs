using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Runtime.Simple;

namespace SAAE.Engine.Mips.Runtime;

public class MachineBuilder {
    private Memory.Memory? _memory;
    private Monocycle? _cpu;
    private MipsOperatingSystem? _os;
    private Stream? _stdin;
    private Stream? _stdout;
    private Stream? _stderr;
    private const ulong Gb = 1024 * 1024 * 1024;
    
    public MachineBuilder With4GbRam() {
        _memory = new Memory.Memory(new MemoryConfiguration() {
            ColdStoragePath = "memory.bin",
            StorageType = StorageType.Volatile,
            ForceColdStorageReset = true,
            PageSize = 4096,
            Size = 4 * Gb,
            MaxLoadedPages = 64,
            Endianess = Endianess.BigEndian
        });
        return this;
    }

    public MachineBuilder WithMipsMonocycle() {
        _cpu = new Monocycle();
        return this;
    }
    
    public MachineBuilder WithMarsOs() {
        _os = new Mars();
        return this;
    }

    public MachineBuilder WithInMemoryStdio() {
        _stdin = new MemoryStream();
        _stdout = new MemoryStream();
        _stderr = new MemoryStream();
        return this;
    }

    public MachineBuilder WithStdio(Stream stdin, Stream stdout, Stream stderr) {
        _stdin = stdin;
        _stdout = stdout;
        _stderr = stderr;
        return this;
    }

    public Machine Build() {
        if (_memory is null) {
            throw new InvalidOperationException("Memory must be set.");
        }
        if (_cpu is null) {
            throw new InvalidOperationException("CPU must be set.");
        }
        if (_os is null) {
            throw new InvalidOperationException("OS must be set.");
        }
        
        Machine machine = new() {
            Cpu = _cpu,
            Memory = _memory,
            Os = _os,
            StdIn = _stdin ?? Stream.Null,
            StdOut = _stdout ?? Stream.Null,
            StdErr = _stderr ?? Stream.Null
        };
        _cpu.Memory = _memory;
        _os.Machine = machine;
        
        _cpu.OnSignalException += (_, e) => {
            _os.OnSignalBreak(e);
        };
        return machine;
    }
}