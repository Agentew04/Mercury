using System.Xml;

namespace InstructionCreator {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private readonly List<Instruction> instructions = [];

        private void AddInstructionButton_Click(object sender, EventArgs e) {
            Instruction inst = new() {
                Id = idTextbox.Text,
                Mnemonic = mnemonicTextbox.Text,
                Arch = archCombo.Text,
                Type = typeCombo.Text,
                Serializes = new(serializes),
                Parses = new(parses),
                Fullname = nameTextbox.Text,
                Description = descriptionTextbox.Text,
                Usage = usageTextbox.Text,
            };

            instructions.Add(inst);
            instructionsList.Items.Add($"{inst.Mnemonic}");

            // reset all
            idTextbox.Text = "";
            mnemonicTextbox.Text = "";
            archCombo.SelectedIndex = -1;
            typeCombo.SelectedIndex = -1;
            parseCombo.SelectedIndex = -1;
            parseTextbox.Text = "";
            parsingList.Items.Clear();
            parses.Clear();
            serializeCombo.SelectedIndex = -1;
            fixedSerializeCheckbox.Checked = false;
            serializeSizeNumeric.Value = 0;
            serializeValueTextbox.Text = "";
            serializationList.Items.Clear();
            serializes.Clear();
            nameTextbox.Text = "";
            descriptionTextbox.Text = "";
            usageTextbox.Text = "";
        }

        private void ExportInstructionsButton_Click(object sender, EventArgs e) {
            // trigger file dialog
            saveFileDialog.FileName = "instructions.xml";
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.OverwritePrompt = true;
            var result = saveFileDialog.ShowDialog();
            if (result != DialogResult.OK) {
                return;
            }
            string path = saveFileDialog.FileName;

            XmlWriterSettings settings = new() {
                Indent = true,
                IndentChars = "    ",
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace,
            };
            using XmlWriter xml = XmlWriter.Create(path, settings);
            xml.WriteStartDocument();
            xml.WriteStartElement("instructions");
            foreach (var instruction in instructions) {
                xml.WriteStartElement("instruction");
                xml.WriteAttributeString("id", instruction.Id);

                xml.WriteElementString("mnemonic", instruction.Mnemonic);

                xml.WriteStartElement("arch");
                if (instruction.Arch.Contains("mips", StringComparison.CurrentCultureIgnoreCase)) {
                    xml.WriteStartElement("mips");
                    xml.WriteAttributeString("version", instruction.Arch.Replace("MIPS",""));
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

            // reset instructions
            instructionsList.Items.Clear();
            instructions.Clear();
        }

        private void exportPseudoButton_Click(object sender, EventArgs e) {
            // trigger file dialog
            saveFileDialog.FileName = "pseudo_instructions.xml";
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.OverwritePrompt = true;
            var result = saveFileDialog.ShowDialog();
            if (result != DialogResult.OK) {
                return;
            }
            string path = saveFileDialog.FileName;

            XmlWriterSettings settings = new() {
                Indent = true,
                IndentChars = "    ",
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace,
            };
            using XmlWriter xml = XmlWriter.Create(path, settings);
            xml.WriteStartDocument();
            xml.WriteStartElement("pseudoinstructions");
        }

        private readonly List<Parse> parses = [];

        private void AddParseButton_Click(object sender, EventArgs e) {
            Parse p = new(parseCombo.Text, parseTextbox.Text);
            parses.Add(p);
            parsingList.Items.Add($"{p.Type}: {p.Name}");
        }

        private readonly List<Serialize> serializes = [];

        private void AddSerializeButton_Click(object sender, EventArgs e) {
            Serialize s = new(serializeCombo.Text, fixedSerializeCheckbox.Checked, (int)serializeSizeNumeric.Value, serializeValueTextbox.Text, serializeBinaryCheckbox.Checked);
            serializes.Add(s);
            serializationList.Items.Add($"{s.Type}: [{(s.Fixed ? 'X' : ' ')}] ({s.Size}) {s.Value} (B?{(s.IsBinaryValue ? 'Y' : 'N')})");
        }

        private void ParseRemove_Click(object sender, EventArgs e) {
            int idx = parsingList.SelectedIndex;
            if (idx == -1) {
                return;
            }
            parses.RemoveAt(idx);
            parsingList.Items.RemoveAt(idx);
        }

        private void SerializeRemove_Click(object sender, EventArgs e) {
            int idx = serializationList.SelectedIndex;
            if (idx == -1) {
                return;
            }
            serializes.RemoveAt(idx);
            serializationList.Items.RemoveAt(idx);
        }

        private void LoadFileButton_Click(object sender, EventArgs e) {
            var result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK) {
                return;
            }
            string path = openFileDialog.FileName;

            var settings = new XmlReaderSettings {
                IgnoreWhitespace = true,
            };
            using XmlReader xml = XmlReader.Create(path, settings);
            xml.ReadStartElement("instructions");

            do {
                if (xml.NodeType != XmlNodeType.Element) {
                    continue;
                }
                if (xml.Name == "instruction") {
                    Instruction inst = new() {
                        Id = xml.GetAttribute("id") ?? "UNKNOWN",
                    };
                    xml.ReadStartElement("instruction");

                    inst.Mnemonic = xml.ReadElementString("mnemonic");

                    xml.ReadStartElement("arch");
                    if (xml.Name == "mips") {
                        xml.MoveToAttribute("version");
                        inst.Arch = "MIPS" + xml.Value;
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
                    instructionsList.Items.Add($"{inst.Mnemonic}");
                }
            } while (xml.Read());

        }

        private void InstructionsList_SelectedIndexChanged(object sender, EventArgs e) {
            if(instructionsList.SelectedIndex == -1) {
                return;
            }
            Instruction inst = instructions[instructionsList.SelectedIndex];

            idTextbox.Text = inst.Id;
            mnemonicTextbox.Text = inst.Mnemonic;
            archCombo.SelectedIndex = archCombo.Items.IndexOf(inst.Arch);
            typeCombo.SelectedIndex = typeCombo.Items.IndexOf(inst.Type);
            parseCombo.SelectedIndex = -1;
            parseTextbox.Text = "";
            parsingList.Items.Clear();
            parsingList.Items.AddRange(inst.Parses.Select(p => $"{p.Type}: {p.Name}").ToArray());
            parses.Clear();
            parses.AddRange(inst.Parses);
            serializeCombo.SelectedIndex = -1;
            fixedSerializeCheckbox.Checked = false;
            serializeSizeNumeric.Value = 0;
            serializeValueTextbox.Text = "";
            serializationList.Items.Clear();
            serializationList.Items.AddRange(inst.Serializes.Select(s => $"{s.Type}: [{(s.Fixed ? 'X' : ' ')}] ({s.Size}) {s.Value} (B?{(s.IsBinaryValue ? 'Y' : 'N')})").ToArray());
            serializes.Clear();
            serializes.AddRange(inst.Serializes);
            nameTextbox.Text = inst.Fullname;
            descriptionTextbox.Text = inst.Description;
            usageTextbox.Text = inst.Usage;
        }

        private void DeleteInstructionButton_Click(object sender, EventArgs e) {
            int idx = instructionsList.SelectedIndex;
            instructions.RemoveAt(idx);
            instructionsList.Items.RemoveAt(idx);
        }

        private void DeletePseudoButton_Click(object sender, EventArgs e) {
            int idx = pseudoList.SelectedIndex;
            //pseudos.RemoveAt(idx)
            pseudoList.Items.RemoveAt(idx);
        }
    }
}
