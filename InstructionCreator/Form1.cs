using System.Xml;

namespace InstructionCreator {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private readonly List<Instruction> instructions = [];

        private void SyncInstructionItems() {
            instructionsList.Items.Clear();
            instructionsList.Items.AddRange(instructions.Select(x => $"{x.Id} ({x.Mnemonic})").ToArray());
        }

        private void ResetInstructionForm() {
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
            addInstructionButton.Text = "Add";
        }

        private void AddInstructionButton_Click(object sender, EventArgs e) {
            // not selected add new
            if(instructionsList.SelectedIndex == -1) {
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
            } else {
                addInstructionButton.Text = "Add";
                Instruction inst = instructions[instructionsList.SelectedIndex];
                inst.Id = idTextbox.Text;
                inst.Mnemonic = mnemonicTextbox.Text;
                inst.Arch = archCombo.Text;
                inst.Type = typeCombo.Text;
                inst.Serializes = new(serializes);
                inst.Parses = new(parses);
                inst.Fullname = nameTextbox.Text;
                inst.Description = descriptionTextbox.Text;
                inst.Usage = usageTextbox.Text;
                inst.Id = idTextbox.Text;
                inst.Mnemonic = mnemonicTextbox.Text;
                instructionsList.SelectedIndex = -1;
            }
            ResetInstructionForm();
            SyncInstructionItems();
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

            InstructionXmlConverter.WriteInstructions(path, instructions);

            // reset instructions
            instructions.Clear();
            SyncInstructionItems();
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
            var instructionsRead = InstructionXmlConverter.LoadInstructionsFile(path);
            instructions.AddRange(instructionsRead);
            SyncInstructionItems();
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

            addInstructionButton.Text = "Save";
        }

        private void DeleteInstructionButton_Click(object sender, EventArgs e) {
            int idx = instructionsList.SelectedIndex;
            instructions.RemoveAt(idx);
            instructionsList.Items.RemoveAt(idx);
            ResetInstructionForm();
        }

        private void DeletePseudoButton_Click(object sender, EventArgs e) {
            int idx = pseudoList.SelectedIndex;
            //pseudos.RemoveAt(idx)
            pseudoList.Items.RemoveAt(idx);
        }
    }
}
