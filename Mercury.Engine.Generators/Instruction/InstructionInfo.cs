namespace Mercury.Generators.Instruction;


internal readonly record struct InstructionInfo(
    string Namespace,
    string ClassName,
    EquatableArray<FormatInfo> Formats,
    EquatableArray<FieldInfo> Fields) {
    
    // general information
    public readonly string Namespace = Namespace;
    public readonly string ClassName = ClassName;
    
    // formatting information
    public readonly EquatableArray<FormatInfo> Formats = Formats;

    // fields information
    public readonly EquatableArray<FieldInfo> Fields = Fields;
}