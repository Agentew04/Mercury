using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAE.Engine.Mips.Assembler; 

public class SourceTextStage {
    public List<string> SourceText { get; init; } = [];
}

public class MacroSolvingStage {
    public List<string> SourceText { get; init; } = [];
}

public class PseudoInstructionSolvingStage {
    public List<string> SourceText { get; init; } = [];
}

public class SymbolReadingStage {
    public List<string> SourceText { get; init; } = [];

    public Dictionary<string, int> SymbolTable { get; init; } = [];
}

public class AssembleStage {

    public byte[] DataSegment { get; init; } = [];

    public byte[] TextSegment { get; init; } = [];
}