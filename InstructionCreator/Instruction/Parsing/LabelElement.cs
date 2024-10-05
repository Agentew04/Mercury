using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction.Parsing;

/// <summary>
/// Classe que representa uma label no codigo fonte
/// do assembly.
/// </summary>
public class LabelElement : ParseElement {

    [XmlText]
    public string Value { get; set; } = "";

    public LabelElement()
    {
        
    }

    public LabelElement(string value)
    {
        Value = value;
    }
}
