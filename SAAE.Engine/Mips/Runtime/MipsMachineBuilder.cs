using SAAE.Engine.Common.Builders;
using SAAE.Engine.Mips.Runtime.OS;
using SAAE.Engine.Mips.Runtime.Simple;

namespace SAAE.Engine.Mips.Runtime;

public class MipsMachineBuilder : MachineBuilder
{
    // TODO: trocar isso por classe base de cpus
    private Monocycle? _cpu;
    private MipsOperatingSystem? _os;
    
    public MipsMachineBuilder(MachineBuilder builder) : base(builder){}

    public MipsMachineBuilder WithMipsMonocycle()
    {
        _cpu = new Monocycle();
        return this;
    }

    public MipsMachineBuilder WithCpu(Monocycle cpu)
    {
        _cpu = cpu;
        return this;
    }
    
    public MipsMachineBuilder WithMarsOs()
    {
        _os = new Mars();
        return this;
    }

    public MipsMachineBuilder WithOs(MipsOperatingSystem os)
    {
        _os = os;
        return this;
    }

    public override Machine Build()
    {
        if (Memory is null)
        {
            throw new InvalidOperationException("Memory must be set.");
        }
        if (_cpu is null)
        {
            throw new InvalidOperationException("CPU must be set.");
        }
        if (_os is null)
        {
            throw new InvalidOperationException("Operating System must be set.");
        }

        Machine machine = new() {
            Cpu = _cpu,
            Memory = Memory,
            Os = _os,
            StdIn = StdIn ?? Stream.Null,
            StdOut = StdOut ?? Stream.Null,
            StdErr = StdErr ?? Stream.Null,
            Architecture = Architecture.Mips
        };
        
        // realiza links de hardware
        _cpu.Memory = Memory;
        _os.Machine = machine;
        _cpu.OnSignalException += (_, e) => {
            _os.OnSignalBreak(e);
        };
        return machine;
    }
}