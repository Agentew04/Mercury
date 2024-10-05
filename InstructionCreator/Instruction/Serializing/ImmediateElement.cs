using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction.Serializing;

/// <summary>
/// This class represents the immediate field in the
/// instruction. This also support operators
/// </summary>
public class ImmediateElement : SerializeElement {

    /// <summary>
    /// Lista dos operadores aplicados ao imediato desta
    /// instrucao na serializacao.
    /// </summary>
    [XmlArray(ElementName = "operations", IsNullable = true)]
    [XmlArrayItem(Type = typeof(ShiftOperator))]
    [XmlArrayItem(Type = typeof(SliceOperator))]
    [XmlArrayItem(Type = typeof(SubtractOperation))]
    public List<ImmediateOperator> Operators { get; set; } = [];
}
