using System.Xml.Serialization;

namespace InstructionCreator.Instruction;

/// <summary>
/// Classe que representa uma instrucao assembly.
/// Contem orientacoes de como parsear e serializar a instrucao.
/// </summary>
[XmlRoot(ElementName = "instruction")]
public class InstructionDefinition {

    [XmlElement(ElementName = "meta")]
    public InstructionMetadata Metadata { get; set; }

    [XmlElement(ElementName = "parsing")]
    public ParseInfo ParseInfo { get; set; }

    [XmlElement(ElementName = "serialization")]
    public SerializationInfo SerializationInfo { get; set; }
}
