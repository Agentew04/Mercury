using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction.Serializing; 

/// <summary>
/// Elemento que representa um registrador
/// na serializacao de uma instrucao.
/// </summary>
public class RegisterElement : SerializeElement {

    /// <summary>
    /// O texto que representa o registrador a ser
    /// considerado nesse elemento.
    /// </summary>
    [XmlText]
    public string Value { get; set; } = "";
}
