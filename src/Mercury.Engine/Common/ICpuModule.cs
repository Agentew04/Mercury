using Mercury.Engine.Memory;

namespace Mercury.Engine.Common;

public interface ICpuModule : IModule{
    
    public uint ProgramEnd { get; set; }
    
    public int ExitCode { get; }
    
    public RegisterCollection Registers { get; }
    
    public Endianess Endianess { get; set; }
}