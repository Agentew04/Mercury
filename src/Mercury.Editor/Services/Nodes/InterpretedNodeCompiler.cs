using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mercury.Editor.Models.Nodes.DesignTime;
using Mercury.Editor.Models.Nodes.ExecuteTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mercury.Editor.Services.Nodes;

public partial class InterpretedNodeCompiler : INodeCompiler {
    public ICompiledDesign CompileDesign(Design design) {
        Dictionary<DesignBlock, StatementSyntax[]> trees = design.Blocks.Select(block => (block,GetBlockTree(block)))
            .ToDictionary(x => x.block, x => x.Item2);

        List<DesignBlock> topo = GetTopologicalOrder(design);
        
        return new InterpretedDesign(design, trees, topo);
    }
    
    private static StatementSyntax[] GetBlockTree(DesignBlock designBlock, StringBuilder? generatedCode = null) {
        string code = 
             $$"""
               public void Compute() {
                   {{designBlock.Source}}
               }
               """;

        generatedCode?.AppendLine(code);
        
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        SyntaxNode root = tree.GetRoot();

        BlockSyntax block = root.DescendantNodes().OfType<BlockSyntax>().First();
        return block.Statements.ToArray();
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