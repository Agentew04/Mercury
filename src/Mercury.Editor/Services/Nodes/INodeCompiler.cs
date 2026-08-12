using System;
using System.Collections.Generic;
using Mercury.Editor.Models.Node.DesignTime;
using Mercury.Editor.Models.Node.ExecuteTime;

namespace Mercury.Editor.Services.Nodes;

public interface INodeCompiler {
    
    public ICompiledDesign CompileDesign(Design design);

    public List<Diagnostic> Validate(Design design);
}
