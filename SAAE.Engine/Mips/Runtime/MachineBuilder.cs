using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Runtime.Simple;

namespace SAAE.Engine.Mips.Runtime;

public class MachineBuilder {
    private VirtualMemory? _memory;
    private Monocycle? _cpu;
    private OperatingSystem? _os;
    private Stream? stdin;
    private Stream? stdout;
    private Stream? stderr;
    private const ulong Gb = 1024 * 1024 * 1024;
    
    public MachineBuilder With4GbRam() {
        _memory = new VirtualMemory(new VirtualMemoryConfiguration() {
            ColdStoragePath = "memory.bin",
            ColdStorageOptimization = true,
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
        stdin = new MemoryStream();
        stdout = new MemoryStream();
        stderr = new MemoryStream();
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
            StdIn = stdin ?? Stream.Null,
            StdOut = stdout ?? Stream.Null,
            StdErr = stderr ?? Stream.Null
        };
        _cpu.Memory = _memory;
        _os.Machine = machine;
        
        _cpu.OnSignalException += (_, e) => {
            _os.OnSignalBreak(e);
        };
        return machine;
    }
}