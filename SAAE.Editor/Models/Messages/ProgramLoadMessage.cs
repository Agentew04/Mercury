using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging.Messages;
using ELFSharp.ELF;
using SAAE.Editor.Extensions;
using SAAE.Engine.Mips.Runtime;

namespace SAAE.Editor.Models.Messages;

public class ProgramLoadMessage
{
    public required MipsMachine MipsMachine { get; init; }
    
    public required ELF<uint> Elf { get; init; }
    public required ProgramMetadata Metadata { get; init; }
}

public class ProgramMetadata {

    public required IReadOnlyList<Symbol> Symbols { get; init; }
    
    public required IReadOnlyList<ObjectFile> Files { get; init; }

    public IEnumerable<Symbol> GetUserDefinedSymbols() {
        return Symbols.Where(x => !x.Name.StartsWith("__") && !x.Name.StartsWith("L.") && x.Name != "_gp");
    }
}

public record struct Symbol(string Name, uint Address);
public record struct ObjectFile(PathObject Path, uint StartAddress, int Index);