using System.Text.RegularExpressions;

namespace SAAE.Engine.Mips.Instructions;

public class Swc1 : TypeIInstruction
{
    public byte Base => Rs;
    public byte Ft => Rt;
    
    public override Regex GetRegularExpression()
    {
        throw new NotSupportedException();
    }

    public override string ToString() => $"swc1 ${TranslateRegisterName(Base)}, {Immediate:X4}(${TypeFInstruction.TranslateRegisterName(Ft)})" + FormatTrivia();
}