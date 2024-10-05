using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction.Parsing;

/// <summary>
/// Element that represents the mnemonic in the source code.
/// </summary>
public class MnemonicElement : ParseElement {

    [XmlText]
    public string Value { get; set; } = "";

    public MnemonicElement()
    {
        
    }

    public MnemonicElement(string value)
    {
        Value = value;
    }
}
