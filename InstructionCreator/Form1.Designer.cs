namespace InstructionCreator {
    partial class Form1 {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            addSerializeButton = new Button();
            addInstructionButton = new Button();
            usageTextbox = new TextBox();
            label12 = new Label();
            descriptionTextbox = new TextBox();
            label11 = new Label();
            nameTextbox = new TextBox();
            label10 = new Label();
            label9 = new Label();
            serializationList = new ListBox();
            serializeSizeNumeric = new NumericUpDown();
            fixedSerializeCheckbox = new CheckBox();
            serializeCombo = new ComboBox();
            label8 = new Label();
            parseTextbox = new TextBox();
            label7 = new Label();
            parseCombo = new ComboBox();
            addParseButton = new Button();
            parsingList = new ListBox();
            label6 = new Label();
            typeCombo = new ComboBox();
            label5 = new Label();
            archCombo = new ComboBox();
            mnemonicTextbox = new TextBox();
            label4 = new Label();
            idTextbox = new TextBox();
            label3 = new Label();
            tabPage2 = new TabPage();
            instructionsList = new ListBox();
            label1 = new Label();
            label2 = new Label();
            pseudoList = new ListBox();
            exportInstructionsButton = new Button();
            exportPseudoButton = new Button();
            saveFileDialog = new SaveFileDialog();
            serializeValueTextbox = new TextBox();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)serializeSizeNumeric).BeginInit();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(598, 375);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(serializeValueTextbox);
            tabPage1.Controls.Add(addSerializeButton);
            tabPage1.Controls.Add(addInstructionButton);
            tabPage1.Controls.Add(usageTextbox);
            tabPage1.Controls.Add(label12);
            tabPage1.Controls.Add(descriptionTextbox);
            tabPage1.Controls.Add(label11);
            tabPage1.Controls.Add(nameTextbox);
            tabPage1.Controls.Add(label10);
            tabPage1.Controls.Add(label9);
            tabPage1.Controls.Add(serializationList);
            tabPage1.Controls.Add(serializeSizeNumeric);
            tabPage1.Controls.Add(fixedSerializeCheckbox);
            tabPage1.Controls.Add(serializeCombo);
            tabPage1.Controls.Add(label8);
            tabPage1.Controls.Add(parseTextbox);
            tabPage1.Controls.Add(label7);
            tabPage1.Controls.Add(parseCombo);
            tabPage1.Controls.Add(addParseButton);
            tabPage1.Controls.Add(parsingList);
            tabPage1.Controls.Add(label6);
            tabPage1.Controls.Add(typeCombo);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(archCombo);
            tabPage1.Controls.Add(mnemonicTextbox);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(idTextbox);
            tabPage1.Controls.Add(label3);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(590, 347);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Instrucao";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // addSerializeButton
            // 
            addSerializeButton.Location = new Point(543, 39);
            addSerializeButton.Name = "addSerializeButton";
            addSerializeButton.Size = new Size(41, 23);
            addSerializeButton.TabIndex = 12;
            addSerializeButton.Text = "Add";
            addSerializeButton.UseVisualStyleBackColor = true;
            addSerializeButton.Click += addSerializeButton_Click;
            // 
            // addInstructionButton
            // 
            addInstructionButton.Location = new Point(421, 199);
            addInstructionButton.Name = "addInstructionButton";
            addInstructionButton.Size = new Size(75, 23);
            addInstructionButton.TabIndex = 16;
            addInstructionButton.Text = "Add";
            addInstructionButton.UseVisualStyleBackColor = true;
            addInstructionButton.Click += addInstructionButton_Click;
            // 
            // usageTextbox
            // 
            usageTextbox.Location = new Point(396, 170);
            usageTextbox.Name = "usageTextbox";
            usageTextbox.Size = new Size(100, 23);
            usageTextbox.TabIndex = 15;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(351, 173);
            label12.Name = "label12";
            label12.Size = new Size(39, 15);
            label12.TabIndex = 22;
            label12.Text = "Usage";
            // 
            // descriptionTextbox
            // 
            descriptionTextbox.Location = new Point(210, 170);
            descriptionTextbox.Name = "descriptionTextbox";
            descriptionTextbox.Size = new Size(135, 23);
            descriptionTextbox.TabIndex = 14;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(137, 173);
            label11.Name = "label11";
            label11.Size = new Size(67, 15);
            label11.TabIndex = 20;
            label11.Text = "Description";
            // 
            // nameTextbox
            // 
            nameTextbox.Location = new Point(51, 170);
            nameTextbox.Name = "nameTextbox";
            nameTextbox.Size = new Size(80, 23);
            nameTextbox.TabIndex = 13;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(6, 173);
            label10.Name = "label10";
            label10.Size = new Size(39, 15);
            label10.TabIndex = 18;
            label10.Text = "Name";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(6, 150);
            label9.Name = "label9";
            label9.Size = new Size(32, 15);
            label9.TabIndex = 17;
            label9.Text = "Help";
            // 
            // serializationList
            // 
            serializationList.FormattingEnabled = true;
            serializationList.ItemHeight = 15;
            serializationList.Location = new Point(247, 68);
            serializationList.Name = "serializationList";
            serializationList.Size = new Size(296, 79);
            serializationList.TabIndex = 16;
            serializationList.TabStop = false;
            // 
            // serializeSizeNumeric
            // 
            serializeSizeNumeric.Location = new Point(447, 39);
            serializeSizeNumeric.Name = "serializeSizeNumeric";
            serializeSizeNumeric.Size = new Size(40, 23);
            serializeSizeNumeric.TabIndex = 10;
            // 
            // fixedSerializeCheckbox
            // 
            fixedSerializeCheckbox.AutoSize = true;
            fixedSerializeCheckbox.Location = new Point(387, 41);
            fixedSerializeCheckbox.Name = "fixedSerializeCheckbox";
            fixedSerializeCheckbox.Size = new Size(54, 19);
            fixedSerializeCheckbox.TabIndex = 9;
            fixedSerializeCheckbox.Text = "Fixed";
            fixedSerializeCheckbox.UseVisualStyleBackColor = true;
            // 
            // serializeCombo
            // 
            serializeCombo.FormattingEnabled = true;
            serializeCombo.Items.AddRange(new object[] { "Opcode", "Register", "Shamt", "Function", "Immediate" });
            serializeCombo.Location = new Point(323, 39);
            serializeCombo.Name = "serializeCombo";
            serializeCombo.Size = new Size(58, 23);
            serializeCombo.TabIndex = 8;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(247, 42);
            label8.Name = "label8";
            label8.Size = new Size(70, 15);
            label8.TabIndex = 12;
            label8.Text = "Serialization";
            // 
            // parseTextbox
            // 
            parseTextbox.Location = new Point(128, 39);
            parseTextbox.Name = "parseTextbox";
            parseTextbox.Size = new Size(45, 23);
            parseTextbox.TabIndex = 6;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(3, 42);
            label7.Name = "label7";
            label7.Size = new Size(46, 15);
            label7.TabIndex = 11;
            label7.Text = "Parsing";
            // 
            // parseCombo
            // 
            parseCombo.FormattingEnabled = true;
            parseCombo.Items.AddRange(new object[] { "Mnemonic", "Register", "Label", "Immediate" });
            parseCombo.Location = new Point(55, 39);
            parseCombo.Name = "parseCombo";
            parseCombo.Size = new Size(67, 23);
            parseCombo.TabIndex = 5;
            // 
            // addParseButton
            // 
            addParseButton.Location = new Point(179, 39);
            addParseButton.Name = "addParseButton";
            addParseButton.Size = new Size(41, 23);
            addParseButton.TabIndex = 7;
            addParseButton.Text = "Add";
            addParseButton.UseVisualStyleBackColor = true;
            addParseButton.Click += addParseButton_Click;
            // 
            // parsingList
            // 
            parsingList.FormattingEnabled = true;
            parsingList.ItemHeight = 15;
            parsingList.Location = new Point(6, 68);
            parsingList.Name = "parsingList";
            parsingList.Size = new Size(214, 79);
            parsingList.TabIndex = 8;
            parsingList.TabStop = false;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(370, 9);
            label6.Name = "label6";
            label6.Size = new Size(31, 15);
            label6.TabIndex = 7;
            label6.Text = "Type";
            // 
            // typeCombo
            // 
            typeCombo.FormattingEnabled = true;
            typeCombo.Items.AddRange(new object[] { "R", "I", "J" });
            typeCombo.Location = new Point(407, 6);
            typeCombo.Name = "typeCombo";
            typeCombo.Size = new Size(54, 23);
            typeCombo.TabIndex = 4;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(247, 9);
            label5.Name = "label5";
            label5.Size = new Size(32, 15);
            label5.TabIndex = 5;
            label5.Text = "Arch";
            // 
            // archCombo
            // 
            archCombo.FormattingEnabled = true;
            archCombo.Items.AddRange(new object[] { "I", "II", "III", "IV", "V", "32", "64" });
            archCombo.Location = new Point(285, 6);
            archCombo.Name = "archCombo";
            archCombo.Size = new Size(79, 23);
            archCombo.TabIndex = 3;
            // 
            // mnemonicTextbox
            // 
            mnemonicTextbox.Location = new Point(174, 6);
            mnemonicTextbox.Name = "mnemonicTextbox";
            mnemonicTextbox.Size = new Size(67, 23);
            mnemonicTextbox.TabIndex = 2;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(103, 9);
            label4.Name = "label4";
            label4.Size = new Size(65, 15);
            label4.TabIndex = 2;
            label4.Text = "Mnemonic";
            // 
            // idTextbox
            // 
            idTextbox.Location = new Point(30, 6);
            idTextbox.Name = "idTextbox";
            idTextbox.Size = new Size(67, 23);
            idTextbox.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 9);
            label3.Name = "label3";
            label3.Size = new Size(18, 15);
            label3.TabIndex = 0;
            label3.Text = "ID";
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(590, 347);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Pseudo";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // instructionsList
            // 
            instructionsList.FormattingEnabled = true;
            instructionsList.ItemHeight = 15;
            instructionsList.Location = new Point(616, 30);
            instructionsList.Name = "instructionsList";
            instructionsList.Size = new Size(172, 139);
            instructionsList.TabIndex = 1;
            instructionsList.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(616, 9);
            label1.Name = "label1";
            label1.Size = new Size(112, 15);
            label1.TabIndex = 2;
            label1.Text = "Current Instructions";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(616, 201);
            label2.Name = "label2";
            label2.Size = new Size(89, 15);
            label2.TabIndex = 3;
            label2.Text = "Current Pseudo";
            // 
            // pseudoList
            // 
            pseudoList.FormattingEnabled = true;
            pseudoList.ItemHeight = 15;
            pseudoList.Location = new Point(616, 219);
            pseudoList.Name = "pseudoList";
            pseudoList.Size = new Size(172, 139);
            pseudoList.TabIndex = 4;
            pseudoList.TabStop = false;
            // 
            // exportInstructionsButton
            // 
            exportInstructionsButton.Location = new Point(616, 175);
            exportInstructionsButton.Name = "exportInstructionsButton";
            exportInstructionsButton.Size = new Size(172, 23);
            exportInstructionsButton.TabIndex = 5;
            exportInstructionsButton.Text = "Export To File";
            exportInstructionsButton.UseVisualStyleBackColor = true;
            exportInstructionsButton.Click += exportInstructionsButton_Click;
            // 
            // exportPseudoButton
            // 
            exportPseudoButton.Location = new Point(616, 364);
            exportPseudoButton.Name = "exportPseudoButton";
            exportPseudoButton.Size = new Size(172, 23);
            exportPseudoButton.TabIndex = 7;
            exportPseudoButton.Text = "Export To File";
            exportPseudoButton.UseVisualStyleBackColor = true;
            exportPseudoButton.Click += exportPseudoButton_Click;
            // 
            // serializeValueTextbox
            // 
            serializeValueTextbox.Location = new Point(492, 38);
            serializeValueTextbox.Name = "serializeValueTextbox";
            serializeValueTextbox.RightToLeft = RightToLeft.No;
            serializeValueTextbox.Size = new Size(45, 23);
            serializeValueTextbox.TabIndex = 11;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 398);
            Controls.Add(exportPseudoButton);
            Controls.Add(exportInstructionsButton);
            Controls.Add(pseudoList);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(instructionsList);
            Controls.Add(tabControl1);
            Name = "Form1";
            Text = "Form1";
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)serializeSizeNumeric).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private Label label5;
        private ComboBox archCombo;
        private TextBox mnemonicTextbox;
        private Label label4;
        private TextBox idTextbox;
        private Label label3;
        private TabPage tabPage2;
        private ListBox instructionsList;
        private Label label1;
        private Label label2;
        private ListBox pseudoList;
        private Button exportInstructionsButton;
        private Label label6;
        private ComboBox typeCombo;
        private Button exportPseudoButton;
        private ComboBox parseCombo;
        private Button addParseButton;
        private ListBox parsingList;
        private TextBox parseTextbox;
        private Label label7;
        private Label label9;
        private ListBox serializationList;
        private NumericUpDown serializeSizeNumeric;
        private CheckBox fixedSerializeCheckbox;
        private ComboBox serializeCombo;
        private Label label8;
        private TextBox usageTextbox;
        private Label label12;
        private TextBox descriptionTextbox;
        private Label label11;
        private TextBox nameTextbox;
        private Label label10;
        private Button addInstructionButton;
        private SaveFileDialog saveFileDialog;
        private Button addSerializeButton;
        private TextBox serializeValueTextbox;
    }
}
