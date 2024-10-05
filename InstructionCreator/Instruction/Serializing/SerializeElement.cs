using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction.Serializing;

/// <summary>
/// Classe base para elementos da serializacao de uma instrucao.
/// </summary>
public abstract class SerializeElement {

    /// <summary>
    /// If the content of this element is always that content
    /// or it can be changed. If this is <see langword="false"/>,
    /// the value refers to a reference to some element.
    /// </summary>
    [XmlAttribute(AttributeName = "fixed")]
    public bool IsFixed { get; set; } = false;

    /// <summary>
    /// The size in bits of this element in the final serialization.
    /// </summary>
    [XmlAttribute(AttributeName = "size")]
    public int Size { get; set; } = 0;

    /// <summary>
    /// Defines if the content of this serialize is 
    /// a binary number or not.
    /// </summary>
    [XmlAttribute(AttributeName = "binary")]
    public bool IsBinary { get; set; } = false;
}
