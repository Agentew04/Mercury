using InstructionCreator.Instruction.Serializing;
using System.Xml.Serialization;

namespace InstructionCreator.Instruction;

/// <summary>
/// Classe que agrupa todas as informacoes necessarias para serializar uma instrucao
/// no seu formato binario.
/// </summary>
public class SerializationInfo {
    [XmlArray(ElementName = "elements")]
    [XmlArrayItem(Type = typeof(MnemonicElement), ElementName = "mnemonic")]
    public List<SerializeElement> Elements { get; set; } = [];

}
