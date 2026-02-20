using System.Collections.Generic;
using Mercury.Generators.Registers;

namespace Mercury.Generators.Architecture;

internal readonly record struct GroupInfo {
    public readonly string Architecture;
    public readonly int Coprocessor;
    public readonly EquatableArray<RegisterInfo> Registers;
    public readonly string EnumTypeName;
    public readonly string? ProcessorName;
    public readonly string Name;

    public GroupInfo(string architecture, int coprocessor, string type, List<RegisterInfo> registers, string? processorName, string name) {
        Architecture = architecture;
        Coprocessor = coprocessor;
        Registers = new EquatableArray<RegisterInfo>(registers.ToArray());
        EnumTypeName = type;
        ProcessorName = processorName;
        Name = name;
    }
}