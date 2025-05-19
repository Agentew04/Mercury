using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Runtime.Simple.PipelineStages;

namespace SAAE.Engine.Mips.Runtime.Simple;

/// <summary>
/// Simple version of the MIPS CPU but with a
/// pipeline. It aims to be a cycle accurate simulator
/// </summary>
public class SimplePipeline : IClockable
{
    public SimplePipeline()
    {
        RegisterFile[RegisterFile.Register.Sp] = 0x7FFF_EFFC; // o 'E' aparece no MARS
        RegisterFile[RegisterFile.Register.Fp] = 0x0000_0000;
        RegisterFile[RegisterFile.Register.Gp] = 0x1000_8000;
        RegisterFile[RegisterFile.Register.Ra] = 0x0000_0000;
        RegisterFile[RegisterFile.Register.Pc] = 0x0040_0000;
    }

    #region Components

    /// <summary>
    /// Represents the RAM of this cpu. It is normally
    /// set from a <see cref="MachineBuilder"/>.
    /// </summary>
    public VirtualMemory Memory { get; set; } = null!;

    /// <summary>
    /// Structure that holds all the general purpose
    /// registers of the CPU.
    /// </summary>
    public RegisterFile RegisterFile { get; private set; } = new();

    #endregion

    #region Properties

    /// <summary>
    /// The first invalid address that the program cannot execute.
    /// </summary>
    public uint DropoffAddress { get; set; } = 0;
    
    /// <summary>
    /// Defines if the cpu is halted or not.
    /// </summary>
    private bool isHalted = false;

    private PipelineStage fetchStage;
    private PipelineStage idStage;
    private PipelineStage executeStage;
    private PipelineStage memoryStage;
    private PipelineStage writebackStage;
        
    #endregion
    
    #region Public Methods

    public void Clock()
    {
        if (isHalted) {
            return;
        }

                    
        
        
    }

    public bool IsClockingFinished()
    {
        return RegisterFile[RegisterFile.Register.Pc] >= DropoffAddress
               || isHalted;
    }

    #endregion
    
    #region Helper Functions
    
    #endregion
}