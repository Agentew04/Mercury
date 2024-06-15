using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine; 

/// <summary>
/// A base class for assemblers that indicates the two pass assembler functions.
/// </summary>
public abstract class Assembler {

    /// <summary>
    /// Main function to compile a source code. Executes the entire pipeline.
    /// </summary>
    /// <param name="code">The assembly source code</param>
    /// <returns>The binary compiled result</returns>
    // TODO replace with a program class packing text and data sections with lazy arrays
    public abstract byte[] Assemble(string code);

    #region Pipeline

    /// <summary>
    /// Inputs the source code text in the assembler.
    /// </summary>
    /// <param name="code"></param>
    protected abstract void InputText(string code);

    /// <summary>
    /// Reads symbols such as labels and macros from the code.
    /// </summary>
    protected abstract void ReadSymbols();

    /// <summary>
    /// Assembles the instructions from the source code with the symbol table 
    /// and proccesses directives.
    /// </summary>
    protected abstract void AssembleInstructions();

    #endregion
}