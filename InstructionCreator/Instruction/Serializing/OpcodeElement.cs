using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction.Serializing; 

/// <summary>
/// Element que representa o opcode dessa instrucao em binario.
/// </summary>
public class OpcodeElement : SerializeElement {

    /// <summary>
    /// The value of this opcode.
    /// </summary>
    [XmlText]
    public int Value { get; set; } = 0;

    public OpcodeElement()
    {
        
    }

    public OpcodeElement(int opcode)
    {
        Value = opcode;
    }
}
