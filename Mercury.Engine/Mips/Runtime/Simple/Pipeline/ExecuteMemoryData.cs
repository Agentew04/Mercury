namespace Mercury.Engine.Mips.Runtime.Simple.Pipeline;

public class ExecuteMemoryData {
    #region Loopback
    
    public uint AluResult { get; set; }
    
    public bool BranchTaken { get; set; }
    
    #endregion
    
    public uint RtValue { get; set; }
    
    public int WriteBackRegister { get; set; }
}