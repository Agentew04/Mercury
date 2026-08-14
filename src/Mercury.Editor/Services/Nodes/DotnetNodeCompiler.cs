using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Mercury.Editor.Models.Nodes.DesignTime;
using Mercury.Editor.Models.Nodes.ExecuteTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.Services.Nodes;

public partial class DotnetNodeCompiler : BaseService<DotnetNodeCompiler>, INodeCompiler {
    public ICompiledDesign CompileDesign(Design design) {
        StringBuilder genCode = new();
        List<SyntaxTree> trees = [];
        Dictionary<DesignBlock, string> blockNames = new();
        trees.AddRange(design.Blocks.Select(block => GetBlockTree(block, blockNames, genCode)));
        trees.Add(GetDesignTree(design, blockNames, out _, genCode));
        //generatedCode = genCode.ToString();

        string assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? throw new Exception("Assembly path not found");
        var namedRefs = new[] {
            "System.Private.CoreLib.dll",
            "System.Console.dll",
            "System.Runtime.dll",
        }.Select(x => MetadataReference.CreateFromFile(Path.Combine(assemblyPath, x)));
        var asmRefs = new[] {
            // typeof(Logger).Assembly
            Assembly.GetExecutingAssembly()
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
            foreach (Microsoft.CodeAnalysis.Diagnostic diag in result.Diagnostics) {
                Logger.LogError("{Location}: {Id}: {Message}", diag.Location, diag.Id, diag.GetMessage());
            }
            return null!;
        }

        ms.Seek(0, SeekOrigin.Begin);
        BlockLoadContext ctx = new(Assembly.GetExecutingAssembly().Location);
        Assembly asm = ctx.LoadFromStream(ms);
        ms.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes("./generated.dll", ms.ToArray());

        ctx.Unloading += ctx => Console.WriteLine("Unloading assembly: " + compilation.AssemblyName);
        return new DotnetCompiledDesign(design, asm, ctx);
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

        // Console.WriteLine("Topological order: ");
        // foreach (DesignBlock block in topo) {
        //     Console.WriteLine("\t- " + block.Name);
        // }

        return topo;
    }

    public List<Diagnostic> Validate(Design design) {
        List<Diagnostic> diags = [];
        
        // size and signedness compatibility
        foreach(Connection conn in design.Connections) {
            var start = conn.Start.Outputs[conn.StartOutputIndex];
            var end = conn.End.Inputs[conn.EndInputIndex];
            if (start.Signed != end.Signed || start.Size != end.Size) {
                diags.Add(new Diagnostic(DiagnosticType.ConnectionBetweenTwoSizes, null, [start, end], [ conn ]));
            }
        }

        // max 1 input for each output
        foreach (var block in design.Blocks) {
            foreach (IoItem input in block.Inputs) {
                List<Connection> conns = design.Connections
                    .Where(c => c.End == block && c.EndInputIndex == block.Inputs.IndexOf(input))
                    .ToList();
                if (conns.Count > 1) {
                    diags.Add(new Diagnostic(DiagnosticType.MultiDrivenInput, null, [input, ..conns.Select(x => x.Start.Outputs[x.StartOutputIndex])], [..conns]));
                }
            }
        }
        
        // cant have duplicate block names
        if (design.Blocks.Select(b => b.Name).Distinct().Count() != design.Blocks.Count) {
            foreach (DesignBlock block in design.Blocks) {
                string name = block.Name;
                IEnumerable<DesignBlock> blocks = design.Blocks.Where(x => x != block && x.Name == name);
                diags.Add(new Diagnostic(DiagnosticType.DuplicatedBlockName, [block,..blocks], null, null));
            }
        }
        
        // each block must have unique ios
        foreach (var block in design.Blocks) {
            // inputs
            if (block.Inputs.Select(i => i.Name).Distinct().Count() != block.Inputs.Count) {
                foreach (IoItem input in block.Inputs) {
                    string name = input.Name;
                    IEnumerable<IoItem> inputs = block.Inputs.Where(x => x != input && x.Name == name);
                    diags.Add(new Diagnostic(DiagnosticType.DuplicateInputName, null, [input, ..inputs], null));
                }
            }
            // outputs
            if (block.Outputs.Select(i => i.Name).Distinct().Count() != block.Outputs.Count) {
                foreach (IoItem output in block.Outputs) {
                    string name = output.Name;
                    IEnumerable<IoItem> outputs = block.Outputs.Where(x => x != output && x.Name == name);
                    diags.Add(new Diagnostic(DiagnosticType.DuplicateOutputName, null, [ output, ..outputs ], null));
                }
            }
        }
        
        // each block must have valid names(identifier rules)
        foreach (DesignBlock block in design.Blocks) {
            Regex regex = IdRegex();
            if (!regex.IsMatch(block.Name)) {
                diags.Add(new Diagnostic(DiagnosticType.InvalidBlockName, [ block ] , null, null));
            }
            if (block.Inputs.Any(input => !regex.IsMatch(input.Name))) {
                foreach (IoItem input in block.Inputs) {
                    if (regex.IsMatch(input.Name)) {
                        continue;
                    }
                    diags.Add(new Diagnostic(DiagnosticType.InvalidInputName, null, [ input ], null));
                }
            }
            if (block.Outputs.Any(output => !regex.IsMatch(output.Name))) {
                foreach (IoItem output in block.Outputs) {
                    if (regex.IsMatch(output.Name)) {
                        continue;
                    }
                    diags.Add(new Diagnostic(DiagnosticType.InvalidOutputName, null, [ output ], null));
                }
            }
        }

        return diags;
    }
    
    [GeneratedRegex("^[_A-Za-z][_A-Za-z0-9]*$")]
    private static partial Regex IdRegex();
}