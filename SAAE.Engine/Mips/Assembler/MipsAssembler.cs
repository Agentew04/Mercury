using SAAE.Engine.Mips.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Assembler; 
public partial class MipsAssembler {

    private readonly List<Instruction> supportedInstructions = [];
    private static readonly string[] branchMnemonics = ["beq", "bgez", "bgtz", "blez", "bltz", "bne"];

    public MipsAssembler() {
        RegisterTypeR();
        RegisterTypeI();
        RegisterTypeJ();
    }

    private static int TranslateRegisterName(string name) {
        string[] names = [
            "zero", "at", "v0", "v1", "a0", "a1", "a2", "a3", "t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7", "s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7", "t8", "t9", "k0", "k1", "gp", "sp", "fp", "ra"
        ];
        return Array.IndexOf(names, name);
    }

    public byte[] Assemble(string code) {
        ulong codeStart = 0x00400000;
        var lines = code.Split('\n');

        // remove empty lines
        lines = lines
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToArray();

        // proccess comments
        var comments = new List<string>();
        var processedLines = new List<string>();
        foreach (var line in lines) {
            var commentIndex = line.IndexOf('#');
            if (commentIndex != -1) {
                comments.Add(line[(commentIndex + 1)..].Trim());
                var codePart = line[..commentIndex].Trim();
                if (!string.IsNullOrWhiteSpace(codePart)) {
                    processedLines.Add(codePart);
                }
            } else {
                processedLines.Add(line);
                comments.Add("");
            }
        }

        lines = processedLines.ToArray();

        // detect labels and save on table
        Dictionary<string, int> labels = [];
        for (int i = 0; i < lines.Length; i++) {
            var line = lines[i];
            if (line.Contains(':')) {
                var parts = line.Split(':');
                labels[parts[0].Trim()] = (int)(codeStart + (ulong)((i) * 4));
                lines[i] = parts[1].Trim();
            }
        }

        // translate labels on instructions
        for (int i = 0; i < lines.Length; i++) {
            var line = lines[i];
            string mnemonic = line.Split(' ')[0];
            // jump, label has the address((addr/4)[:26])
            if (mnemonic == "j" || mnemonic == "jal") {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (labels.TryGetValue(parts[1], out int value)) {
                    parts[1] = value.ToString();
                }
                lines[i] = string.Join(' ', parts);
            }
            // branch, label has the offset((addr/4)[:16])
            if (branchMnemonics.Contains(mnemonic)) {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // if any of the parts is equal of any label
                string? label = Array.Find(parts, labels.ContainsKey);
                if(label is not null) {
                    int value = labels[label];
                    value = (value - (int)(codeStart + (ulong)((i+1) * 4))) >> 2;
                    int partIndex = Array.IndexOf(parts, label); // poderiamos fazer uma busca so em vez de 2
                    parts[partIndex] = value.ToString();
                }
                lines[i] = string.Join(' ', parts);
            }
        }

        // translate register names to numbers
        for (int i = 0; i < lines.Length; i++) {
            var line = lines[i];
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < parts.Length; j++) {
                if (parts[j][0] == '$' && !char.IsDigit(parts[j][1])) {
                    parts[j] = "$" + TranslateRegisterName(parts[j][1..].Replace(",",""));
                }
            }
            lines[i] = (parts[0] + " " + string.Join(", ", parts[1..])).Replace(",,",",");
        }

        List<Instruction> instructions = [];
        for (int i = 0; i < lines.Length; i++) {
            var line = lines[i];
            var baseInstruction = supportedInstructions.Find(x => x.IsMatch(line)) ?? throw new Exception($"Instruction not supported: {line}");
            Instruction? instruction = (Instruction?)Activator.CreateInstance(baseInstruction.GetType());
            if(instruction is null) {
                throw new Exception($"Could not create instance of instruction {baseInstruction.GetType().FullName}");
            }
            instruction.PopulateFromLine(line);
            instruction.Address = (int)(codeStart + (ulong)(i * 4));
            instruction.CommentTrivia = comments[i];
            instructions.Add(instruction);
        }

        List<byte> bytes = [];

        foreach (var instruction in instructions) {
            bytes.AddRange(BitConverter.GetBytes(instruction.ConvertToInt()));
        }

        
        return bytes.ToArray();
    }
}
