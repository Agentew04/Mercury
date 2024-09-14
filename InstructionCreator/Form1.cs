namespace InstructionCreator {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private readonly List<Instruction> instructions = [];

        private void addInstructionButton_Click(object sender, EventArgs e) {
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

        private void exportInstructionsButton_Click(object sender, EventArgs e) {
            // trigger file dialog
            saveFileDialog.FileName = "instructions.xml";
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.OverwritePrompt = true;
            var result = saveFileDialog.ShowDialog();
            if (result != DialogResult.OK) {
                return;
            }
            string path = saveFileDialog.FileName;
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
        }

        private readonly List<Parse> parses = [];

        private void addParseButton_Click(object sender, EventArgs e) {
            Parse p = new(typeCombo.Text, parseTextbox.Text);
            parses.Add(p);
            parsingList.Items.Add($"{p.Type}: {p.Name}");
        }

        private readonly List<Serialize> serializes = [];

        private void addSerializeButton_Click(object sender, EventArgs e) {
            Serialize s = new(serializeCombo.Text, fixedSerializeCheckbox.Checked, (int)serializeSizeNumeric.Value, serializeValueTextbox.Text);
            serializes.Add(s);
            serializationList.Items.Add($"{s.Type}: [{(s.Fixed ? 'X' : ' ')}] ({s.Size}) {s.Value}");
        }
    }
}
