namespace Mercury.Generators.Instruction;

internal readonly record struct FormatterMethodInfo(
    string Specifier,
    string Namespace,
    string ClassName,
    string MethodName
) {
    public readonly string Specifier = Specifier;
    public readonly string Namespace = Namespace;
    public readonly string ClassName = ClassName;
    public readonly string MethodName = MethodName;
}
