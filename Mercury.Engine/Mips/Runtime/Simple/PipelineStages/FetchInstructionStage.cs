namespace Mercury.Engine.Mips.Runtime.Simple.PipelineStages;

public class FetchInstructionStage : PipelineStage
{
    public Func<bool> PcSrc { get; set; }
    
    public Func<uint> JumpAddress { get; set; }
    
    public Func<uint> GetPc { get; set; }
    
    public Action<uint> SetPc { get; set; }
    
    public Func<uint, uint> GetInstruction { get; set; }
    
    public override void Clock()
    {
        bool pcSrc = PcSrc();
        
        // last pc + 4
        uint pcPlus4 = GetPc() + 4;

        // pc + 4
        var newpc = pcSrc ? JumpAddress() : pcPlus4;
        SetPc(newpc);
        
        var instruction = GetInstruction(newpc);
    }
}