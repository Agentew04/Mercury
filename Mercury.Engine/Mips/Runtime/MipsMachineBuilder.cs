using Mercury.Engine.Common;
using Mercury.Engine.Common.Builders;
using Mercury.Engine.Mips.Runtime.OS;
using Mercury.Engine.Mips.Runtime.Simple;

namespace Mercury.Engine.Mips.Runtime;

public class MipsMachineBuilder : MachineBuilder
{
    private IMipsCpu? cpu;
    private MipsOperatingSystem? os;
    
    public MipsMachineBuilder(MachineBuilder builder) : base(builder){}

    public MipsMachineBuilder WithMipsMonocycle()
    {
        cpu = new Monocycle();
        return this;
    }

    public MipsMachineBuilder WithMipsPipeline() {
        cpu = new SimplePipeline();
        return this;
    }

    public MipsMachineBuilder WithCpu(IMipsCpu cpu)
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
        if (DataMemory is null)
        {
            throw new InvalidOperationException("Data Memory must be set.");
        }

        if (InstructionMemory is null) {
            throw new InvalidOperationException("Instruction Memory must be set.");
        }
        if (cpu is null)
        {
            throw new InvalidOperationException("CPU must be set.");
        }
        if (os is null)
        {
            throw new InvalidOperationException("Operating System must be set.");
        }

        MipsMachine mipsMachine = new(cpu, os) {
            DataMemory = DataMemory,
            InstructionMemory = InstructionMemory,
            StdIn = StdIn ?? new NullChannel<char>(),
            StdOut = StdOut ?? new NullChannel<char>(),
            StdErr = StdErr ?? new NullChannel<char>(),
            Architecture = Architecture.Mips
        };
        
        // realiza links de hardware
        cpu.Machine = mipsMachine;
        os.Machine = mipsMachine;
        cpu.SignalException += async e => {
            await os.OnSignalBreak(e);
        };
        return mipsMachine;
    }
}