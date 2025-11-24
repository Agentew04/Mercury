using Mercury.Engine.Mips.Runtime;

namespace Mercury.Engine.Common;

public interface ICpu : IAsyncClockable {
    
    public Machine Machine { get; set; }
    
    public uint ProgramEnd { get; set; }
    
    public RegisterCollection Registers { get; }
    
    public int ExitCode { get; }
    
    public ValueTask Halt(int code = 0);
}