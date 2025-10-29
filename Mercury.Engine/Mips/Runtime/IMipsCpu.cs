using Mercury.Engine.Common;
using Mercury.Engine.Mips.Runtime;
using Mercury.Engine.Common;

namespace Mercury.Engine.Mips.Runtime;

public interface IMipsCpu : ICpu {
    
    public MipsMachine MipsMachine { get; set; }

    Machine ICpu.Machine {
        get => MipsMachine;
        set => MipsMachine = (MipsMachine)value;
    }
    
    public event Func<SignalExceptionEventArgs, Task>? SignalException;
}