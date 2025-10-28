using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;


namespace Mercury.Engine.Mips.Instructions; 
public class InstructionFactory {

    private List<Rule> rules;

    public InstructionFactory() {
        using Stream? s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mercury.Engine.Mips.Instructions.Disassembly_Rules.json");
        if(s is null) {
            throw new Exception("Could not find the disassembly rules file.");
        }

        rules = JsonSerializer.Deserialize(s, SerializerContext.Default.RuleList) ?? [];
        if(rules.Count == 0) {
            throw new Exception("No rules read from file!");
        }
    }

    public class RuleList : List<Rule> {}
    
    public record Rule {

        [JsonPropertyName("mnemonic")]
        [JsonPropertyOrder(0)]
        public string Mnemonic { get; set; } = "";

        [JsonPropertyName("constraints")]
        [JsonPropertyOrder(1)]
        public Dictionary<string, List<uint>> Constraints { get; set; } = [];
    }

    [UnconditionalSuppressMessage("Trimming", "IL2057")]
    public Instruction Disassemble(uint binary) {
        if (binary == 0) {
            return new Nop();
        }
        
        uint opcode = binary >> 26;
        IEnumerable<Rule>? oprules = rules.Where(x => x.Constraints.ContainsKey("opcode") && x.Constraints["opcode"].Contains(opcode));
        foreach(Rule? rule in oprules) {
            // checar o resto das constraints
            bool failed = false;
            foreach(string? constraint in rule.Constraints.Keys) {
                if(constraint == "opcode") {
                    continue;
                }
                switch (constraint) {
                    case "funct":
                        uint funct = (binary & 0x3F);
                        if(!rule.Constraints[constraint].Contains(funct)) {
                            failed = true;
                        }
                        break;
                    case "shift":
                        uint shift = (binary >> 6) & 0x1F;
                        if(!rule.Constraints[constraint].Contains(shift)) {
                            failed = true;
                        }
                        break;
                    case "rs":
                        uint rs = (binary >> 21) & 0x1F;
                        if(!rule.Constraints[constraint].Contains(rs)) {
                            failed = true;
                        }
                        break;
                    case "rt":
                        uint rt = (binary >> 16) & 0x1F;
                        if(!rule.Constraints[constraint].Contains(rt)) {
                            failed = true;
                        }
                        break;
                    case "rd":
                        uint rd = (binary >> 11) & 0x1F;
                        if(!rule.Constraints[constraint].Contains(rd)) {
                            failed = true;
                        }
                        break;
                    case "nd":
                        uint nd = (binary >> 17) & 1;
                        if(!rule.Constraints[constraint].Contains(nd)) {
                            failed = true;
                        }
                        break;
                    case "tf":
                        uint tf = (binary >> 16) & 1;
                        if(!rule.Constraints[constraint].Contains(tf)) {
                            failed = true;
                        }
                        break;
                    case "zfc":
                        uint zfc = (binary >> 4) & 0b1111;
                        if(!rule.Constraints[constraint].Contains(zfc)) {
                            failed = true;
                        }
                        break;
                    case "op4pre":
                        uint op4 = binary >> 28;
                        if(!rule.Constraints[constraint].Contains(op4)) {
                            failed = true;
                        }
                        break;
                }
                if (failed) {
                    break;
                }
            }
            if (failed) {
                continue;
            }

            // se este tipo nao existir, temos problemas muito maiores...
            // pode criar instancia aqui
            if(Activator.CreateInstance(Type.GetType($"Mercury.Engine.Mips.Instructions.{rule.Mnemonic}")!) is not Instruction instruction) {
                throw new Exception("No rule matched this instruction!");
            }
            instruction.FromInt((int)binary);
            return instruction;
        }
        throw new Exception("No rule matched this instruction!");
    }
}

[JsonSerializable(typeof(InstructionFactory.Rule))]
[JsonSerializable(typeof(InstructionFactory.RuleList))]
public partial class SerializerContext : JsonSerializerContext {

}