using System.Collections.Generic;

namespace Mercury.Generators.Architecture;

internal readonly record struct ArchitecturesInfo {
    public readonly string EnumFullname;
    public readonly EquatableArray<string> Architectures;

    public ArchitecturesInfo(string fullname, List<string> architectures) {
        EnumFullname = fullname;
        Architectures = new EquatableArray<string>(architectures.ToArray());
    }
}