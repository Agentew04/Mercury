using SAAE.Engine.Mips.Instructions;

namespace SAAE.Engine.Mips.Runtime.Simple.Pipeline;

public class DecodeExecuteData {
    
    public Instruction Instruction { get; set; }
    
    public int RsValue { get; set; }
    
    public int RtValue { get; set; }
    
    public int WriteBackRegister { get; set; }
    
    public int ImmediateExtended { get; set; }
}