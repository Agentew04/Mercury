using InstructionCreator.Instruction;
using InstructionCreator.Instruction.Parsing;
using InstructionCreator.Instruction.Serializing;
using System.Xml.Serialization;

namespace InstructionCreator {
    internal static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {

            InstructionMetadata meta = new() {
                Id = "add",
                Mnemonic = "add",
                Isa = ISA.MIPS_I,
                Size = 32,
                Type = InstructionType.RType
            };
            Instruction.InstructionDefinition inst = new() {
                Metadata = meta,
                ParseInfo = new() {
                    Elements = [
                        new MnemonicElement("add"),
                        new CommaElement(),
                        new Instruction.Parsing.RegisterElement("rd"),
                        new CommaElement(),
                        new Instruction.Parsing.RegisterElement("rs"),
                        new CommaElement(),
                        new Instruction.Parsing.RegisterElement("rt"),
                    ]
                },
                SerializationInfo = new() {
                    Elements = [
                        new OpcodeElement(){
                            Size = 6,
                            IsFixed = true,
                            Value = 0,
                            IsBinary = false
                        },
                        new Instruction.Serializing.RegisterElement(){
                            Size = 5,
                            IsFixed = false,
                            IsBinary = false,
                            Value = "rs"
                        },
                        new Instruction.Serializing.RegisterElement(){
                            Size = 5,
                            IsFixed = false,
                            IsBinary = false,
                            Value = "rt"
                        },
                        new Instruction.Serializing.RegisterElement(){
                            Size = 5,
                            IsFixed = false,
                            IsBinary = false,
                            Value = "rd"
                        },


                    ]
                }
            };

            XmlSerializer serializer = new(typeof(Instruction.InstructionDefinition));
            serializer.Serialize(Console.Out, inst);

            return;
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}