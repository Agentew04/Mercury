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
                Arch = archCombo.SelectedText,
                Type = typeCombo.SelectedText,
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
            foreach (var instruction in instructions)
            {
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
                foreach(var serialize in instruction.Serializes) {
                    xml.WriteStartElement(serialize.Type);
                    xml.WriteAttributeString("fixed", serialize.Fixed ? "true" : "false");
                    xml.WriteAttributeString("size", serialize.Size.ToString());
                    xml.WriteString(serialize.Value);
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();

                xml.WriteStartElement("parsing");
                foreach(var parse in instruction.Parses) {
                    xml.WriteElementString(parse.Type, parse.Name);
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
            Serialize s = new(serializeCombo.Text, fixedSerializeCheckbox.Checked, (int)serializeSizeNumeric.Value, serializeValueTextbox.Text);
            serializes.Add(s);
            serializationList.Items.Add($"{s.Type}: [{(s.Fixed ? 'X' : ' ')}] ({s.Size}) {s.Value}");
        }
    }
}
