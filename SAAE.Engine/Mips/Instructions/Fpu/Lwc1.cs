using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public class Lwc1 : TypeIInstruction
{
    public byte Base => Rs;
    public byte Ft => Rt;
    
    public Lwc1()
    {
        OpCode = 0b110001;
    }
    
    public override Regex GetRegularExpression()
    {
        throw new NotSupportedException();
    }

    public override string ToString() => $"lwc1 ${TranslateRegisterName(Rt)}, {Immediate:X4}(${TranslateRegisterName(Rs)})" + FormatTrivia();
}