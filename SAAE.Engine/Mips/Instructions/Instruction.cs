using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Instructions; 
public abstract class Instruction {

    public int? Address { get; set; }

    public abstract Regex GetRegularExpression();

    public bool IsMatch(string line) {
        return GetRegularExpression().IsMatch(line);
    }

    public abstract void PopulateFromLine(string line);

    public abstract void FromInt(int instruction);

    public abstract int ConvertToInt();
}
