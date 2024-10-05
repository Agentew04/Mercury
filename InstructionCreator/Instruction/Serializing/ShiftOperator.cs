using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction.Serializing;

/// <summary>
/// Operador de imediato que movimenta os bits para a esquerda ou direita.
/// </summary>
[XmlType(TypeName = "shift")]
public class ShiftOperator : ImmediateOperator {

    [XmlAttribute(AttributeName = "direction")]
    public ShiftDirection Direction { get; set; } = ShiftDirection.Left;

    public enum ShiftDirection {
        Left,
        Right
    }
}
