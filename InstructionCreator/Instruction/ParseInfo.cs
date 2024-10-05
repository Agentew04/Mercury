using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using InstructionCreator.Instruction.Parsing;

namespace InstructionCreator.Instruction;

/// <summary>
/// Classe que guarda os passos para ler essa instrucao
/// a partir do codigo fonte(texto).
/// </summary>
public class ParseInfo
{

    [XmlArray(ElementName = "elements")]
    [XmlArrayItem(Type = typeof(MnemonicElement), ElementName = "mnemonic")]
    [XmlArrayItem(Type = typeof(CommaElement), ElementName = "comma")]
    [XmlArrayItem(Type = typeof(RegisterElement), ElementName = "register")]
    [XmlArrayItem(Type = typeof(LabelElement), ElementName = "label")]
    [XmlArrayItem(Type = typeof(ImmediateElement), ElementName = "immediate")]
    public List<ParseElement> Elements { get; set; } = [];
}
