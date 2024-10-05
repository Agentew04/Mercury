using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction; 

/// <summary>
/// Classe que guarda os metadados de uma instrucao.
/// </summary>
public class InstructionMetadata {

    /// <summary>
    /// The unique identifier of the instruction. The app must not
    /// accept two instructions with the same id.
    /// </summary>
    [XmlElement(ElementName = "id")]
    public string Id { get; set; } = "";

    /// <summary>
    /// The mnemonic of this instruction.
    /// </summary>
    [XmlElement(ElementName = "mnemonic")]
    public string Mnemonic { get; set; } = "";

    /// <summary>
    /// The textform of this ISA.
    /// </summary>
    [XmlElement(ElementName = "isa")]
    public ISA Isa { get; set; } = ISA.Unknown;

    /// <summary>
    /// The total size in bits of this instruction.
    /// </summary>
    [XmlElement(ElementName = "size")]
    public int Size { get; set; } = 0;

    /// <summary>
    /// The type of this instruction.
    /// </summary>
    [XmlElement(ElementName = "type")]
    public InstructionType Type { get; set; } = InstructionType.RType;

    public override bool Equals(object? obj) {
        if (obj is not InstructionMetadata other) {
            return false;
        }
        return Id == other.Id && Mnemonic == other.Mnemonic && Isa == other.Isa && Size == other.Size;
    }
}
