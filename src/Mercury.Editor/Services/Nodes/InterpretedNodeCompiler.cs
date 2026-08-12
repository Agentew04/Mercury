using System.Collections.Generic;
using Mercury.Editor.Models.Node.DesignTime;
using Mercury.Editor.Models.Node.ExecuteTime;

namespace Mercury.Editor.Services.Nodes;

public class InterpretedNodeCompiler : INodeCompiler {
    public ICompiledDesign CompileDesign(Design design) {
        throw new System.NotImplementedException();
    }

    public List<Diagnostic> Validate(Design design) {
        throw new System.NotImplementedException();
    }
}