using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

/// <summary>
/// Represents a floating-point instruction in the MIPS architecture.
/// </summary>
/// <remarks>This is not a real type of instruction. Added to the code
/// only for simplicity and abstraction.</remarks>
public abstract class TypeFInstruction : Instruction
{
    protected TypeFInstruction()
    {
        OpCode = 0b010001;
    }

    public const byte SinglePrecisionFormat = 0b10000;
    public const byte DoublePrecisionFormat = 0b10001;//10100
    public const byte FixedPrecisionFormat = 0b10100;

    public override Regex GetRegularExpression()
    {
        throw new NotSupportedException();
    }

    public override void PopulateFromLine(string line)
    {
        throw new NotSupportedException();
    }
    
    protected new static int TranslateRegisterName(string name)
    {
        return int.Parse(name[1..]);
    }
    
    protected new static string TranslateRegisterName(int index)
    {
        return $"f{index}";
    }

    protected static string FormatFmt(byte fmt)
    {
        return fmt switch
        {
            SinglePrecisionFormat => "s",
            DoublePrecisionFormat => "d",
            FixedPrecisionFormat => "w",
            _ => throw new ArgumentOutOfRangeException(nameof(fmt), "Invalid format code.")
        };
    }
}