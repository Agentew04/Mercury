using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace InstructionCreator;

public static class InstructionXmlConverter {

    public static List<Instruction1> LoadInstructionsFile(string xmlPath) {
        var settings = new XmlReaderSettings {
            IgnoreWhitespace = true,
        };
        using XmlReader xml = XmlReader.Create(xmlPath, settings);

        List<Instruction1> instructions = [];

        xml.ReadStartElement("instructions");
        do {
            if (xml.NodeType != XmlNodeType.Element) {
                continue;
            }
            if (xml.Name == "instruction") {
                Instruction1 inst = new() {
                    Id = xml.GetAttribute("id") ?? "UNKNOWN",
                };
                xml.ReadStartElement("instruction");

                inst.Mnemonic = xml.ReadElementString("mnemonic");

                xml.ReadStartElement("arch");
                if (xml.Name == "mips") {
                    xml.MoveToAttribute("version");
                    inst.Arch = xml.Value;
                    xml.Read();
                }
                xml.ReadEndElement();

                inst.Type = xml.ReadElementString("type");

                xml.ReadStartElement("serialization");
                while (xml.Name != "serialization" && xml.NodeType != XmlNodeType.EndElement) {
                    string? fixedStr = xml.GetAttribute("fixed");
                    string? sizeStr = xml.GetAttribute("size");
                    string? binaryStr = xml.GetAttribute("binary");
                    Serialize s = new(xml.Name, bool.Parse(fixedStr ?? "false"), int.Parse(sizeStr ?? "0"), xml.ReadElementContentAsString(), bool.Parse(binaryStr ?? "false"));
                    inst.Serializes.Add(s);
                }
                xml.ReadEndElement();

                xml.ReadStartElement("parsing");
                while (xml.Name != "parsing" && xml.NodeType != XmlNodeType.EndElement) {
                    string name = xml.Name;
                    Parse p = new(name, xml.ReadElementContentAsString());
                    inst.Parses.Add(p);
                }
                xml.ReadEndElement();

                xml.ReadStartElement("help");
                inst.Fullname = xml.ReadElementString("fullname");
                inst.Description = xml.ReadElementString("description");
                inst.Usage = xml.ReadElementString("usage");
                xml.ReadEndElement();

                instructions.Add(inst);
            }
        } while (xml.Read());
        return instructions;
    }

    public static void WriteInstructions(string xmlPath, List<Instruction1> instructions) {
        XmlWriterSettings settings = new() {
            Indent = true,
            IndentChars = "    ",
            NewLineChars = Environment.NewLine,
            NewLineHandling = NewLineHandling.Replace,
        };
        using XmlWriter xml = XmlWriter.Create(xmlPath, settings);
        xml.WriteStartDocument();
        xml.WriteStartElement("instructions");
        foreach (var instruction in instructions) {
            xml.WriteStartElement("instruction");
            xml.WriteAttributeString("id", instruction.Id);

            xml.WriteElementString("mnemonic", instruction.Mnemonic);

            xml.WriteStartElement("arch");
            if (instruction.Arch.Contains("mips", StringComparison.CurrentCultureIgnoreCase)) {
                xml.WriteStartElement("mips");
                xml.WriteAttributeString("version", instruction.Arch);
                xml.WriteEndElement();
            }
            xml.WriteEndElement();

            xml.WriteElementString("type", instruction.Type);

            xml.WriteStartElement("serialization");
            xml.WriteAttributeString("length", 32.ToString());
            foreach (var serialize in instruction.Serializes) {
                xml.WriteStartElement(serialize.Type);
                xml.WriteAttributeString("fixed", serialize.Fixed ? "true" : "false");
                xml.WriteAttributeString("size", serialize.Size.ToString());
                xml.WriteAttributeString("binary", serialize.IsBinaryValue ? "true" : "false");
                if (!string.IsNullOrWhiteSpace(serialize.Value)) {
                    xml.WriteString(serialize.Value);
                }
                xml.WriteEndElement();
            }
            xml.WriteEndElement();

            xml.WriteStartElement("parsing");
            foreach (var parse in instruction.Parses) {
                xml.WriteElementString(parse.Type.ToLower(), parse.Name);
            }
            xml.WriteEndElement();

            xml.WriteStartElement("help");
            xml.WriteElementString("fullname", instruction.Fullname);
            xml.WriteElementString("description", instruction.Description);
            xml.WriteElementString("usage", instruction.Usage);
            xml.WriteEndElement();

            xml.WriteEndElement();
        }
        xml.WriteEndElement();
        xml.WriteEndDocument();
    }
}
