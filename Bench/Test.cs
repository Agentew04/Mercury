using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using TypeInfo = System.Reflection.TypeInfo;

namespace Bench;

/*
 * use <SatelliteResourceLanguages>en-US</SatelliteResourceLanguages>
 * on csproj to avoid satellite resource assemblies for Roslyn lib
 */

public static class Logger {
    public static void Log<T>(T o) {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {o}");
    }
}

public partial class Test {
    public static void A() {
        Design design = CreateDesign();
        (Assembly asm, BlockLoadContext ctx)? result = CompileDesign(design, out string code);

        if (!result.HasValue) {
            Console.WriteLine("Erro de compilacao");
            int i = 0;
            foreach(string line in code.Split('\n')) {
                Console.WriteLine($"{i+1}\t{line}");
                i++;
            }
            return;
        }

        Console.WriteLine("Design compiled successfully");
        Console.WriteLine("Generated code:");
        Console.WriteLine(code);
        
        Assembly asm = result.Value.asm;
        GeneratedDesign generatedDesign = new(design, asm);
        for (int i = 0; i < 10; i++) {
            generatedDesign.Tick();
        }
    }

    private static Design CreateDesign() {
        DesignBlock register = new("counterRegister",
            [new IoItem("input", 32, false)],
            [new IoItem("output", 32, false)],
            true,
            "output.output = input.input; Log($\"Register computed: {output.output}\");");

        DesignBlock adder1 = new("add1",
            [new IoItem("input", 32, false)],
            [new IoItem("output", 32, false)],
            false,
            "output.output = input.input + 1;");
        DesignBlock adder2 = new("add2",
            [new IoItem("input", 32, false)],
            [new IoItem("output", 32, false)],
            false,
            "output.output = input.input + 1;");
        DesignBlock adder3 = new("add3",
            [new IoItem("input", 32, false)],
            [new IoItem("output", 32, false)],
            false,
            "output.output = input.input + 1;");
        DesignBlock adder4 = new("add4", [
                new IoItem("a", 32, false),
                new IoItem("b", 32, false),
                new IoItem("c", 32, false),
                new IoItem("d", 32, false)
            ],
            [new IoItem("result", 32, false)],
            false,
            "output.result = input.a + input.b + input.c + input.d;");

        List<Connection> conns = [
            new(register, 0, adder1, 0),
            new(register, 0, adder2, 0),
            new(register, 0, adder3, 0),
            new(adder1, 0, adder4, 0),
            new(adder2, 0, adder4, 1),
            new(adder3, 0, adder4, 2),
            new(register, 0, adder4, 3),
            new(adder4, 0, register, 0),
        ];
        return new Design() {
            Blocks = [register, adder1, adder2, adder3, adder4],
            Connections = conns,
        };
    }

    private static (Assembly asm, BlockLoadContext ctx)? CompileDesign(Design design, out string generatedCode) {
        if (!ValidateDesign(design)) {
            Console.WriteLine("Invalid design");
            generatedCode = string.Empty;
            return null;
        }
        
        StringBuilder genCode = new();
        List<SyntaxTree> trees = [];
        Dictionary<DesignBlock, string> blockNames = new();
        trees.AddRange(design.Blocks.Select(block => GetBlockTree(block, blockNames, genCode)));
        trees.Add(GetDesignTree(design, blockNames, out _, genCode));
        generatedCode = genCode.ToString();

        string assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? throw new Exception("Assembly path not found");
        var namedRefs = new[] {
            "System.Private.CoreLib.dll",
            "System.Console.dll",
            "System.Runtime.dll",
        }.Select(x => MetadataReference.CreateFromFile(Path.Combine(assemblyPath, x)));
        var asmRefs = new[] {
            typeof(Logger).Assembly
        }.Select(x => MetadataReference.CreateFromFile(x.Location));
        IEnumerable<MetadataReference> references = namedRefs.Concat(asmRefs);
        
        
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: $"Design_{Random.Shared.Next():X8}",
            syntaxTrees: trees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using MemoryStream ms = new();
        EmitResult result = compilation.Emit(ms);

        if (!result.Success) {
            foreach (Diagnostic diag in result.Diagnostics) {
                Console.WriteLine($"{diag.Location}: {diag.Id}: {diag.GetMessage()}");
            }

            return null;
        }

        ms.Seek(0, SeekOrigin.Begin);
        BlockLoadContext ctx = new(Assembly.GetExecutingAssembly().Location);
        Assembly asm = ctx.LoadFromStream(ms);
        ms.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes("./generated.dll", ms.ToArray());
        return (asm, ctx);
    }

    private static SyntaxTree GetBlockTree(DesignBlock designBlock, Dictionary<DesignBlock, string> blockNames,
        StringBuilder? generatedCode = null) {
        StringBuilder inputSb = new();
        inputSb.AppendLine("    public struct Input {");
        foreach (IoItem input in designBlock.Inputs) {
            inputSb.Append("        public ");
            switch (input.Size) {
                case <= 8:
                    inputSb.Append(input.Signed ? "sbyte " : "byte ");
                    break;
                case <= 16:
                    inputSb.Append(input.Signed ? "short " : "ushort ");
                    break;
                case <= 32:
                    inputSb.Append(input.Signed ? "int " : "uint ");
                    break;
                case <= 64:
                    inputSb.Append(input.Signed ? "long " : "ulong ");
                    break;
                default:
                    throw new Exception("Input size too large");
            }

            inputSb.Append(input.Name);
            inputSb.AppendLine(";");
        }

        inputSb.AppendLine("    }");

        StringBuilder outputSb = new();
        outputSb.AppendLine("    public struct Output {");
        foreach (IoItem input in designBlock.Outputs) {
            outputSb.Append("        public ");
            switch (input.Size) {
                case <= 8:
                    outputSb.Append(input.Signed ? "sbyte " : "byte ");
                    break;
                case <= 16:
                    outputSb.Append(input.Signed ? "short " : "ushort ");
                    break;
                case <= 32:
                    outputSb.Append(input.Signed ? "int " : "uint ");
                    break;
                case <= 64:
                    outputSb.Append(input.Signed ? "long " : "ulong ");
                    break;
                default:
                    throw new Exception("Input size too large");
            }

            outputSb.Append(input.Name);
            outputSb.AppendLine(";");
        }

        outputSb.AppendLine("    }");

        string name = $"{designBlock.Name}_{Random.Shared.Next():X8}";
        blockNames[designBlock] = name;
        
        string code =
            $$"""
              public class {{name}}  {
              {{inputSb}}
              {{outputSb}}
                  public Input input = new();
                  public Output output = new();
                  public Input uncommited = new();
                  private void Log<T>(T o) => Bench.Logger.Log(o);
                  public void Compute() {
                      {{designBlock.Source}}
                  }
              {{(!designBlock.IsBarrier ? "" : 
              """
                  public void Commit(){
                      input = uncommited;
                      uncommited = default;
                  }
              """)}}
              }
              """;

        generatedCode?.AppendLine(code);

        return CSharpSyntaxTree.ParseText(code);
    }

    private static SyntaxTree GetDesignTree(Design design, Dictionary<DesignBlock, string> blockNames,
        out string designName, StringBuilder? generatedCode = null) {
        StringBuilder sb = new();

        List<DesignBlock> topo = GetTopologicalOrder(design);


        designName = $"CompiledDesign_{Random.Shared.Next():X8}";
        sb.AppendLine($"public class {designName} {{");
        foreach (DesignBlock block in design.Blocks) {
            // instantiate blocks
            sb.AppendLine($"    public readonly {blockNames[block]} {block.Name} = new();");
        }

        // tick method
        sb.AppendLine("    public void Tick() {");

        // barriers
        sb.AppendLine("        // compute barriers");
        foreach (DesignBlock barrier in design.Blocks.Where(b => b.IsBarrier)) {
            sb.AppendLine($"        {barrier.Name}.Compute();");
        }

        // combinacional
        Dictionary<Connection, bool> computed = design.Connections.ToDictionary(x => x, _ => false);
        sb.AppendLine("        // compute combinational logic");
        foreach (DesignBlock block in topo) {
            if (block.IsBarrier) {
                continue;
            }

            foreach (Connection incoming in design.Connections.Where(x => x.End == block)) {
                if (computed[incoming]) {
                    continue;
                }
                computed[incoming] = true;
                sb.AppendLine(
                    $"        {incoming.End.Name}.{(incoming.End.IsBarrier ? "uncommited" : "input")}.{incoming.End.Inputs[incoming.EndInputIndex].Name} " +
                    $"= {incoming.Start.Name}.output.{incoming.Start.Outputs[incoming.StartOutputIndex].Name};"
                );
            }

            sb.AppendLine($"        {block.Name}.Compute();");
            foreach (Connection outgoing in design.Connections.Where(x => x.Start == block)) {
                if (computed[outgoing]) {
                    continue;
                }
                computed[outgoing] = true;

                sb.AppendLine(
                    $"        {outgoing.End.Name}.{(outgoing.End.IsBarrier ? "uncommited" : "input")}.{outgoing.End.Inputs[outgoing.EndInputIndex].Name} " +
                    $"= {outgoing.Start.Name}.output.{outgoing.Start.Outputs[outgoing.StartOutputIndex].Name};"
                );
            }
        }

        sb.AppendLine("        // commit barriers");
        foreach (DesignBlock barrier in design.Blocks.Where(b => b.IsBarrier)) {
            sb.AppendLine($"        {barrier.Name}.Commit();");
        }

        sb.AppendLine("    }");

        sb.AppendLine("}");
        generatedCode?.AppendLine(sb.ToString());
        return CSharpSyntaxTree.ParseText(sb.ToString());
    }

    private static List<DesignBlock> GetTopologicalOrder(Design design) {
        Dictionary<DesignBlock, List<DesignBlock>> adjacency = new();
        Dictionary<DesignBlock, int> inDegree = new();
        // build graph
        foreach (DesignBlock block in design.Blocks) {
            adjacency[block] = [];
            inDegree[block] = 0;
        }

        foreach (Connection conn in design.Connections) {
            if (conn.Start.IsBarrier) continue;
            adjacency[conn.Start].Add(conn.End);
            inDegree[conn.End]++;
        }

        // topological sort
        Queue<DesignBlock> queue = new(
            design.Blocks.Where(b => !b.IsBarrier && inDegree[b] == 0)
        );

        List<DesignBlock> topo = [];

        while (queue.Count > 0) {
            DesignBlock b = queue.Dequeue();
            topo.Add(b);

            foreach (DesignBlock next in adjacency[b]) {
                inDegree[next]--;
                if (inDegree[next] == 0)
                    queue.Enqueue(next);
            }
        }

        if (topo.Count != design.Blocks.Count) {
            throw new Exception("Ciclo combinacional detectado no design");
        }

        Console.WriteLine("Topological order: ");
        foreach (DesignBlock block in topo) {
            Console.WriteLine("\t- " + block.Name);
        }

        return topo;
    }
    
    private static bool ValidateDesign(Design design) {
        // size and signedness compatibility
        foreach(Connection conn in design.Connections) {
            var start = conn.Start.Outputs[conn.StartOutputIndex];
            var end = conn.End.Inputs[conn.EndInputIndex];
            if (start.Signed != end.Signed || start.Size != end.Size) {
                Console.WriteLine("size and signedness mismatch on connection from {0}.{1} to {2}.{3}",
                    conn.Start.Name, start.Name, conn.End.Name, end.Name);
                return false;
            }
        }

        // max 1 input for each output
        foreach (var block in design.Blocks) {
            foreach (IoItem input in block.Inputs) {
                int count = design.Connections.Count(c => c.End == block && c.EndInputIndex == block.Inputs.IndexOf(input));
                if (count > 1) {
                    Console.WriteLine("Output {0}.{1} has more than one connection", block.Name, input.Name);
                    return false;
                }
            }
        }
        
        // cant have duplicate block names
        if (design.Blocks.Select(b => b.Name).Distinct().Count() != design.Blocks.Count) {
            Console.WriteLine("Duplicate block names detected");
            return false;
        }
        
        // each block must have unique ios
        foreach (var block in design.Blocks) {
            // inputs
            if (block.Inputs.Select(i => i.Name).Distinct().Count() != block.Inputs.Count) {
                Console.WriteLine("Duplicate input names detected on block {0}", block.Name);
                return false;
            }
            // outputs
            if (block.Outputs.Select(i => i.Name).Distinct().Count() != block.Outputs.Count) {
                Console.WriteLine("Duplicate output names detected on block {0}", block.Name);
                return false;
            }
        }
        
        // each block must have valid names(identifier rules)
        foreach (DesignBlock block in design.Blocks) {
            Regex regex = IdRegex();
            if (!regex.IsMatch(block.Name)) {
                Console.WriteLine("Invalid block name: {0}", block.Name);
                return false;
            }
            if (block.Inputs.Any(input => !regex.IsMatch(input.Name))) {
                Console.WriteLine("Invalid input name on block {0}", block.Name);
                return false;
            }
            if (block.Outputs.Any(output => !regex.IsMatch(output.Name))) {
                Console.WriteLine("Invalid output name on block {0}", block.Name);
                return false;
            }
        }

        return true;
    }

    [GeneratedRegex("^[_A-Za-z][_A-Za-z0-9]*$")]
    private static partial Regex IdRegex();
}

public class BlockLoadContext : AssemblyLoadContext {
    
    private readonly AssemblyDependencyResolver resolver;
    
    public BlockLoadContext(string mainAssemblyPath) : base(isCollectible: true) {
        resolver = new AssemblyDependencyResolver(mainAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName) {
        Assembly? loaded = Default.Assemblies
            .FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
        if (loaded != null) {
            return loaded;
        }
        string? path = resolver.ResolveAssemblyToPath(assemblyName);
        return path != null ? LoadFromAssemblyPath(path) : null;
    }
}

public record IoItem(string Name, int Size, bool Signed);

public record DesignBlock(string Name, List<IoItem> Inputs, List<IoItem> Outputs, bool IsBarrier, string Source);

public record Connection(DesignBlock Start, int StartOutputIndex, DesignBlock End, int EndInputIndex);

public class Design {
    public List<DesignBlock> Blocks { get; set; } = [];
    public List<Connection> Connections { get; set; } = [];
}

public class GeneratedDesign {
    public GeneratedDesign(Design design, Assembly asm) {
        TypeInfo type = asm.DefinedTypes.First(x => x.DeclaredMethods.Any(y => y.Name == "Tick"));
        instance = Activator.CreateInstance(type) ?? throw new Exception("No constructor found");
        MethodInfo? tickMethod = type.GetMethod("Tick");
        tick = tickMethod ?? throw new Exception("No tick method found");

        foreach (DesignBlock block in design.Blocks) {
            FieldInfo blockField = type.GetField(block.Name, BindingFlags.Instance | BindingFlags.Public)
                ?? throw new Exception($"Field {block.Name} not found");
            Type blockType = blockField.FieldType;
            FieldInfo inputField = blockType.GetField("input", BindingFlags.Instance | BindingFlags.Public)
                ?? throw new Exception($"Field input on {blockType.Name} not found");
            FieldInfo outputField = blockType.GetField("output", BindingFlags.Instance | BindingFlags.Public)
                ?? throw new Exception($"Field output on {blockType.Name} not found");
            
            blocks[block] = new RuntimeBlock(blockField.GetValue(instance)!, inputField, outputField);
        }
    }
    
    private record RuntimeBlock(object Instance, FieldInfo InputField, FieldInfo OutputField);

    private readonly object instance;
    private readonly MethodInfo tick;
    private readonly Dictionary<DesignBlock, RuntimeBlock> blocks = [];

    public void Tick() {
        tick.Invoke(instance, []);
    }

    public T GetInputValue<T>(DesignBlock block, IoItem item) {
        (object blockInstance, FieldInfo inputField, _) = blocks[block];
        object inputStruct = inputField.GetValue(blockInstance)!;
        return (T)inputField.FieldType.GetField(item.Name)!.GetValue(inputStruct)!;
    }
    
    public T GetOutputValue<T>(DesignBlock block, IoItem item) {
        (object blockInstance, _, FieldInfo outputField) = blocks[block];
        object outputStruct = outputField.GetValue(blockInstance)!;
        return (T)outputField.FieldType.GetField(item.Name)!.GetValue(outputStruct)!;
    }
}
