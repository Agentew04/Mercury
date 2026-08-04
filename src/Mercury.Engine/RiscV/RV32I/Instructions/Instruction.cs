namespace Mercury.Engine.RiscV.RV32I.Instructions;

/// <summary>
/// Shared class with helper methods that RV32I instructions can use.   
/// </summary>
public static class Instruction {
    
    public static string TranslateRegisterName(int index)
    {
        return $"x{index}";
    }
}