using Mercury.Engine.Common;
using Mercury.Engine.Common.Builders;
using Mercury.Engine.Mips.Runtime.OS;
using Mercury.Engine.Mips.Runtime.Simple;

namespace Mercury.Engine.Mips.Runtime;

public class MipsMachineBuilder : MachineBuilder
{
    private IMipsCpu? cpu;
    private MipsOperatingSystem? os;
    private Func<SignalExceptionEventArgs, Task>? signalHandler;
    private OsType osType = OsType.NotSet;
    
    public MipsMachineBuilder(MachineBuilder builder) : base(builder){}

    public MipsMachineBuilder WithMipsMonocycle()
    {
        cpu = new Monocycle();
        return this;
    }

    // public MipsMachineBuilder WithMipsPipeline() {
    //     cpu = new SimplePipeline();
    //     return this;
    // }

    public MipsMachineBuilder WithCpu(IMipsCpu cpu)
    {
        this.cpu = cpu;
        return this;
    }
    
    public MipsMachineBuilder WithMarsOs()
    {
        if (osType != OsType.NotSet) {
            throw new NotSupportedException("OS type already set.");
        }
        osType = OsType.Named;
        os = new Mars();
        return this;
    }

    public MipsMachineBuilder WithOs(MipsOperatingSystem os)
    {
        if (osType != OsType.NotSet) {
            throw new NotSupportedException("OS type already set.");
        }
        osType = OsType.Named;
        this.os = os;
        return this;
    }

    public MipsMachineBuilder WithBareMetal() {
        if (osType != OsType.NotSet) {
            throw new NotSupportedException("OS type already set.");
        }
        osType = OsType.BareMetal;
        return this;
    }

    public MipsMachineBuilder WithAnonymousOs(Func<SignalExceptionEventArgs,Task> handler) {
        if (osType != OsType.NotSet) {
            throw new NotSupportedException("OS type already set.");
        }
        osType = OsType.Anonymous;
        signalHandler = handler;
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
        if (osType == OsType.NotSet)
        {
            throw new InvalidOperationException("Operating System must be set or use bare metal");
        }

        MipsMachine mipsMachine = new(cpu, os) {
            DataMemory = DataMemory,
            Memory = InstructionMemory,
            StdIn = StdIn ?? new NullChannel<char>(),
            StdOut = StdOut ?? new NullChannel<char>(),
            StdErr = StdErr ?? new NullChannel<char>(),
            Architecture = Architecture.Mips
        };
        
        // realiza links de hardware
        cpu.Machine = mipsMachine;
        if (osType == OsType.Named) {
            os!.Machine = mipsMachine;
            cpu.SignalException += async e => {
                await os.OnSignalBreak(e);
            };
        }else if (osType == OsType.Anonymous) {
            cpu.SignalException += signalHandler;
        }
        return mipsMachine;
    }

    private enum OsType {
        NotSet,
        Named,
        Anonymous,
        BareMetal
    }
}