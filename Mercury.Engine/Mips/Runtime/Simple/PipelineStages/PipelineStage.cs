namespace Mercury.Engine.Mips.Runtime.Simple.PipelineStages;

public abstract class PipelineStage : IClockable
{
    public abstract void Clock();

    public bool IsClockingFinished()
    {
        return false;
    }
}