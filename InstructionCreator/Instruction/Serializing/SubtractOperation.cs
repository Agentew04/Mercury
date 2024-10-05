using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction.Serializing;

/// <summary>
/// Operador de imediato que subtrai um valor do imediato
/// ou vice-versa. Faz o valor ser: <see cref="FirstOperand"/>-<see cref="SecondOperand"/>.
/// </summary>
[XmlType(TypeName = "subtract")]
public class SubtractOperation : ImmediateOperator {

    [XmlAttribute(AttributeName = "first")]
    public string FirstOperand { get; set; } = "";

    [XmlAttribute(AttributeName = "second")]
    public string SecondOperand { get; set; } = "";
}
