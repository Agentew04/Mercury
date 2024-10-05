using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction.Serializing;

/// <summary>
/// Operador do imediato para selecionar apenas
/// parte dos bits para manipulacao.
/// Os parametros <see cref="Start"/> e <see cref="End"/>
/// sao referentes aos bits, do menos significativo ao
/// mais significativo.
/// </summary>
[XmlType(TypeName = "slice")]
public class SliceOperator : ImmediateOperator {

    /// <summary>
    /// O inicio do slice, inclusivo.
    /// </summary>
    [XmlAttribute(AttributeName = "start")]
    public int Start { get; set; } = 0;

    /// <summary>
    /// O fim do slice, inclusivo.
    /// </summary>
    [XmlAttribute(AttributeName = "end")]
    public int End { get; set; } = 0;
}
