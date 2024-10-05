using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction.Parsing;

/// <summary>
/// Parse element that represents a register in the
/// the instruction.
/// </summary>
public class RegisterElement : ParseElement {

    [XmlText]
    public string Value { get; set; } = "";

    public RegisterElement()
    {
        
    }

    public RegisterElement(string value)
    {
        Value = value;
    }
}
