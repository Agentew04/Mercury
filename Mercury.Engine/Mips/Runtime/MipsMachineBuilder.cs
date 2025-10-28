using Mercury.Engine.Common;
using Mercury.Engine.Common.Builders;
using Mercury.Engine.Mips.Runtime.OS;
using Mercury.Engine.Mips.Runtime.Simple;

namespace Mercury.Engine.Mips.Runtime;

public class MipsMachineBuilder : MachineBuilder
{
    private Monocycle? cpu;
    private MipsOperatingSystem? os;
    
    public MipsMachineBuilder(MachineBuilder builder) : base(builder){}

    public MipsMachineBuilder WithMipsMonocycle()
    {
        cpu = new Monocycle();
        return this;
    }

    public MipsMachineBuilder WithCpu(Monocycle cpu)
    {
        this.cpu = cpu;
        return this;
    }
    
    public MipsMachineBuilder WithMarsOs()
    {
        os = new Mars();
        return this;
    }

    public MipsMachineBuilder WithOs(MipsOperatingSystem os)
    {
        this.os = os;
        return this;
    }

    public override MipsMachine Build()
    {
        if (Memory is null)
        {
            throw new InvalidOperationException("Memory must be set.");
        }
        if (cpu is null)
        {
            throw new InvalidOperationException("CPU must be set.");
        }
        if (os is null)
        {
            throw new InvalidOperationException("Operating System must be set.");
        }

        MipsMachine mipsMachine = new() {
            Cpu = cpu,
            Memory = Memory,
            Os = os,
            StdIn = StdIn,
            StdOut = StdOut,
            StdErr = StdErr,
            Architecture = Architecture.Mips
        };
        
        // realiza links de hardware
        cpu.Memory = Memory;
        cpu.MipsMachine = mipsMachine;
        os.Machine = new WeakReference<Machine?>(mipsMachine);
        cpu.OnSignalException += async (e) => {
            await os.OnSignalBreak(e);
        };
        return mipsMachine;
    }
}