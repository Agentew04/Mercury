namespace Mercury.Generators.Instruction;


internal readonly record struct InstructionInfo(
    string Namespace,
    string ClassName,
    EquatableArray<FormatInfo> Formats,
    EquatableArray<FieldInfo> Fields,
    string? AssemblyFormat = null,
    bool HasCustomToString = false) {
    
    // general information
    public readonly string Namespace = Namespace;
    public readonly string ClassName = ClassName;
    
    // formatting information
    public readonly EquatableArray<FormatInfo> Formats = Formats;

    // fields information
    public readonly EquatableArray<FieldInfo> Fields = Fields;

    // assembly information
    public readonly string? AssemblyFormat = AssemblyFormat;
    public readonly bool HasCustomToString = HasCustomToString;
}