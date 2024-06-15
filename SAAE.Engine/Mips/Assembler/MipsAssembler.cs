using SAAE.Engine.Mips.Instructions;
using System.Text.RegularExpressions;

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

    public AssembleStage NewAssemble(string code) {
        var sourceTextStage = InputText(code);

        var macroSolvingStage = ResolveMacros(sourceTextStage);

        var pseudoSolvingStage = ResolvePseudoInstructions(macroSolvingStage);

        var readsymbolsStage = ReadSymbols(pseudoSolvingStage);

        var assembleStage = AssembleInstructions(readsymbolsStage);


        return assembleStage;
    }

    public SourceTextStage InputText(string code) {
        return new() {
            SourceText = code
                .Split('\n')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                //.Select(x => x.Trim())
                .ToList()
        };
    }

    private class Macro {
        public string Name { get; init; } = "";
        public string[] Arguments { get; init; } = [];
        public List<string> Body { get; init; } = [];
    }

    public MacroSolvingStage ResolveMacros(SourceTextStage upstream) {
        List<Macro> macros = [];

        // read macros and remove from source code
        int state = 0;
        // 0 - normal
        // 1 - reading macro
        Macro? currentMacro = null;
        for (int i = 0; i < upstream.SourceText.Count; i++) {
            string line = upstream.SourceText[i];
            int macroDefinitionIndex = line.IndexOf(".macro");
            if (macroDefinitionIndex != -1 && state == 0) { // start of macro
                state = 1;
                int parenthesisIndex = line.IndexOf('(');
                if(parenthesisIndex == -1) {
                    // no arguments
                    currentMacro = new Macro() {
                        Name = line[(macroDefinitionIndex + 7)..].Trim()
                    };
                } else {
                    line = line.Replace(')', ' ');
                    // has arguments
                    currentMacro = new Macro() {
                        Name = line[(macroDefinitionIndex + 7)..parenthesisIndex].Trim(),
                        Arguments = line[(parenthesisIndex + 1)..^1].Split(',').Select(x => x.Trim()).ToArray()
                    };
                }
                upstream.SourceText.RemoveAt(i);
                i--;
                continue;
            } else if (line.Contains(".end_macro") && state == 1) { // macro end
                macros.Add(currentMacro!);
                currentMacro = null;
                upstream.SourceText.RemoveAt(i);
                i--;
                state = 0;
            } else if(state == 1) { // we are reading a macro
                currentMacro!.Body.Add(line.TrimEnd());
                upstream.SourceText.RemoveAt(i);
                i--;
            }
            // not macro related
        }

        // codigo muito feio, talvez refatorar isso depois :(
        // apply macros
        for(int i=0; i<upstream.SourceText.Count; i++) {
            string line = upstream.SourceText[i].TrimStart();
            foreach (var macro in macros) {
                if (line.StartsWith(macro.Name)) {
                    line = line.Split('#')[0].Replace('(', ' ').Replace(')', ' ').Replace(',', ' ');
                    string[] parts = line.Split(' ', StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length - 1 != macro.Arguments.Length) {
                        throw new Exception($"Macro {macro.Name} expects {macro.Arguments.Length} arguments, but {parts.Length - 1} were given.\n" +
                            $"L{i}: {line}\n" +
                            $"Arguments: {string.Join(",", parts)}");
                    }
                    List<string> newLines = [];
                    for (int j = 0; j < macro.Body.Count; j++) {
                        string newLine = macro.Body[j];
                        for (int k = 0; k < macro.Arguments.Length; k++) {
                            newLine = newLine.Replace(macro.Arguments[k], parts[k + 1]);
                        }
                        newLines.Add(newLine);
                    }
                    upstream.SourceText.RemoveAt(i);
                    upstream.SourceText.InsertRange(i, newLines);
                    i += newLines.Count - 1;
                }
            }
        }

        // read eqvs
        Dictionary<string, string> eqvs = [];
        for (int i = 0; i < upstream.SourceText.Count; i++) {
            string line = upstream.SourceText[i];
            int eqvIndex = line.IndexOf(".eqv");
            if (eqvIndex != -1) {
                line = line[(eqvIndex + 4)..].Trim();
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                eqvs[parts[0]] = parts[1];
                upstream.SourceText.RemoveAt(i);
                i--;
            }
        }

        // apply eqvs
        for (int i = 0; i < upstream.SourceText.Count; i++) {
            string line = upstream.SourceText[i];
            foreach (var eqv in eqvs) {
                line = line.Replace(eqv.Key, eqv.Value);
            }
            upstream.SourceText[i] = line;
        }

        return new() {
            SourceText = upstream.SourceText
        };
    }

    public PseudoInstructionSolvingStage ResolvePseudoInstructions(MacroSolvingStage upstream) {
        // do nothing 
        return new() {
            SourceText = upstream.SourceText
        };
    }

    [GeneratedRegex(@"^(?<!#)\s*(?<label>[^\s#]+)\s*(?=:)")]
    private partial Regex LabelRegex();
    public SymbolReadingStage ReadSymbols(PseudoInstructionSolvingStage upstream) {
        Regex labelRegex = LabelRegex();
        Dictionary<string, int> symbolTable = [];
        for (int i=0; i< upstream.SourceText.Count; i++) {
            string line = upstream.SourceText[i];
            Match m = labelRegex.Match(line);
            if(m.Success) {
                // remove label from source code
                string label = m.Groups["label"].Value;
                upstream.SourceText[i] = line[(m.Index + m.Length)..].Trim();
                symbolTable[m.Groups["label"].Value] = i;
            }
        }
        return new() {
            SourceText = upstream.SourceText,
            SymbolTable = symbolTable
        };
    }


    public AssembleStage AssembleInstructions(SymbolReadingStage upstream) {
        //upstream.

        return new();
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
                if (label is not null) {
                    int value = labels[label];
                    value = (value - (int)(codeStart + (ulong)((i + 1) * 4))) >> 2;
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
                    parts[j] = "$" + TranslateRegisterName(parts[j][1..].Replace(",", ""));
                }
            }
            lines[i] = (parts[0] + " " + string.Join(", ", parts[1..])).Replace(",,", ",");
        }

        List<Instruction> instructions = [];
        for (int i = 0; i < lines.Length; i++) {
            var line = lines[i];
            var baseInstruction = supportedInstructions.Find(x => x.IsMatch(line)) ?? throw new Exception($"Instruction not supported: {line}");
            Instruction? instruction = (Instruction?)Activator.CreateInstance(baseInstruction.GetType());
            if (instruction is null) {
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
