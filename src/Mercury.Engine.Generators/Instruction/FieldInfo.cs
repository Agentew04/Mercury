namespace Mercury.Generators.Instruction;

internal readonly record struct FieldInfo(int BitStart, int BitEnd, string FieldType, string FieldName, bool IsSigned) {
    public readonly int BitStart = BitStart;
    public readonly int BitEnd = BitEnd;
    public readonly string FieldType = FieldType;
    public readonly string FieldName = FieldName;
    public readonly bool IsSigned = IsSigned;
}