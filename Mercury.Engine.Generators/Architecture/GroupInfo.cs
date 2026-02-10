using System.Collections.Generic;

namespace Mercury.Generators.Architecture;

internal readonly record struct GroupInfo {
    public readonly string Architecture;
    public readonly int Coprocessor;
    public readonly EquatableArray<RegisterDef> Registers;
    public readonly string EnumTypeName;

    public GroupInfo(string architecture, int coprocessor, string type, List<RegisterDef> registers) {
        Architecture = architecture;
        Coprocessor = coprocessor;
        Registers = new EquatableArray<RegisterDef>(registers.ToArray());
        EnumTypeName = type;
    }
}