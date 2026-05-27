using Mercury.Engine.Common;
using Mercury.Engine.Common.Builders;
using Mercury.Engine.Memory;
using Mercury.Engine.Mips.Runtime.OS;
using Mercury.Engine.Mips.Runtime.Simple;
using Mercury.Engine.Modules.Gpu;
using Mercury.Engine.Modules.Gpu.Configs;
using Mercury.Engine.Modules.Gpu.Events;

namespace Mercury.Engine.Mips.Runtime;

public class MipsMachineBuilder : MachineBuilder {
    private ICpuModule? cpu;
    private MipsSyscallModule? os;
    private OsType osType = OsType.NotSet;

    public MipsMachineBuilder(MachineBuilder builder) : base(builder) {
    }

    public MipsMachineBuilder WithMipsMonocycle() {
        cpu = new Monocycle();
        Modules.Add(new Monocycle());
        return this;
    }

    public MipsMachineBuilder WithCpu(ICpuModule cpu) {
        this.cpu = cpu;
        Modules.Add(cpu);
        return this;
    }

    public MipsMachineBuilder WithMarsOs() {
        if (osType != OsType.NotSet) {
            throw new NotSupportedException("OS type already set.");
        }

        osType = OsType.Named;
        os = new Mars();
        Modules.Add(os);
        return this;
    }

    public MipsMachineBuilder WithOs(MipsSyscallModule os) {
        if (osType != OsType.NotSet) {
            throw new NotSupportedException("OS type already set.");
        }

        osType = OsType.Named;
        this.os = os;
        Modules.Add(os);
        return this;
    }

    public MipsMachineBuilder WithBareMetal() {
        if (osType != OsType.NotSet) {
            throw new NotSupportedException("OS type already set.");
        }

        osType = OsType.BareMetal;
        return this;
    }

    public MipsMachineBuilder With<TModule>() where TModule : IModule, new() {
        TModule module = new();
        module.Map(AddressDecoderModule);
        Modules.Add(module);
        return this;
    }
    
    public MipsMachineBuilder With<TModule, TConfig>(TConfig config) where TModule : IConfigurableModule<TConfig>, new() {
        TModule module = new();
        module.Configure(config);
        module.Map(AddressDecoderModule);
        Modules.Add(module);
        return this;
    }
    
    public override MipsMachine Build() {
        if (Memory is null) {
            throw new InvalidOperationException("Data Memory must be set.");
        }

        if (cpu is null) {
            throw new InvalidOperationException("CPU must be set.");
        }

        if (osType == OsType.NotSet) {
            throw new InvalidOperationException("Operating System must be set or use bare metal");
        }

        foreach (IModule module in Modules) {
            module.SubscribeToEvents(EventBus);
        }
        
        AddressDecoderModule.BuildMappings();

        MipsMachine mipsMachine = new() {
            EventBus = EventBus,
            Modules = Modules,
            Architecture = Architecture.Mips
        };

        return mipsMachine;
    }

    private enum OsType {
        NotSet,
        Named,
        Anonymous,
        BareMetal
    }
}