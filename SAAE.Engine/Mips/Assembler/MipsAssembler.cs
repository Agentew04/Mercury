using SAAE.Engine.Mips.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Assembler; 
public partial class MipsAssembler {

    private readonly List<Instruction> supportedInstructions = [];

    public MipsAssembler() {
        RegisterTypeR();
        RegisterTypeI();
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

        // remove comments
        for (int i = 0; i < lines.Length; i++) {
            var commentIndex = lines[i].IndexOf('#');
            if (commentIndex != -1) {
                lines[i] = lines[i][..commentIndex];
            }
            lines[i] = lines[i].Trim();
        }

        // remove empty lines
        lines = lines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

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
            var instruction = supportedInstructions.Find(x => x.IsMatch(line)) ?? throw new Exception($"Instruction not supported: {line}");
            instruction.PopulateFromLine(line);
            instruction.Address = (int)(codeStart + (ulong)(i * 4));
            instructions.Add(instruction);
        }

        List<byte> bytes = [];

        foreach (var instruction in instructions) {
            bytes.AddRange(BitConverter.GetBytes(instruction.ConvertToInt()));
        }

        
        return bytes.ToArray();
    }
}
