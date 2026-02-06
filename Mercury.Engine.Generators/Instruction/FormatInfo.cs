using System.Collections.Generic;

namespace Mercury.Generators;

internal enum FormatInfoType {
    Unknown,
    Exact,
    Different
}

internal readonly record struct FormatInfo {
    public readonly FormatInfoType InfoType;
    public readonly int BitStart;
    public readonly int BitEnd;
    public readonly EquatableArray<int> Values;

    public FormatInfo(FormatInfoType infoType, int bitStart, int bitEnd, List<int> values) {
        InfoType = infoType;
        BitStart = bitStart;
        BitEnd = bitEnd;
        Values = new EquatableArray<int>(values.ToArray());
    }
}