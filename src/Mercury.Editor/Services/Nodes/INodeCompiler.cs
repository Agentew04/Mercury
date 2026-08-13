using System;
using System.Collections.Generic;
using Mercury.Editor.Models.Nodes.DesignTime;
using Mercury.Editor.Models.Nodes.ExecuteTime;

namespace Mercury.Editor.Services.Nodes;

/// <summary>
/// Interface that specifies functions for a service that can compile a node based design in
/// an executable format.
/// </summary>
public interface INodeCompiler {
    
    /// <summary>
    /// Compile a node based design into an executable format.
    /// </summary>
    /// <param name="design">The design to compile</param>
    /// <returns>The executable format for the given design</returns>
    public ICompiledDesign CompileDesign(Design design);

    /// <summary>
    /// Checks if there is any semantic errors in the design.
    /// </summary>
    /// <param name="design">The design to validate</param>
    /// <returns>A list with the errors found. If no errors are found, an empty list is found</returns>
    public List<Diagnostic> Validate(Design design);
}
