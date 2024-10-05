using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction.Parsing; 

/// <summary>
/// A class to represent a immediate in the
/// source code assembly of a instruction
/// </summary>
public class ImmediateElement : ParseElement {

    /// <summary>
    /// Defines the maximum size in bits that the value
    /// of this immediate can have
    /// </summary>
    [XmlAttribute("size")]
    public int Size { get; set; } = 0;

    /// <summary>
    /// Defines if this immediate is considered
    /// a signed number or an unsigned one.
    /// </summary>
    [XmlAttribute("signed")]
    public bool IsSigned { get; set; } = false;

    /// <summary>
    /// The value of the immediate
    /// </summary>
    [XmlText]
    public int Value { get; set; } = 0;
}
