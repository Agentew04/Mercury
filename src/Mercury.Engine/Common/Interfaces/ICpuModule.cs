namespace Mercury.Engine.Common;

public interface ICpuModule : IModule{
    
    public uint ProgramEnd { get; set; }
    
    public int ExitCode { get; }
    
    public RegisterCollection Registers { get; }
}