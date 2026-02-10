namespace Mercury.Generators.Architecture;

internal readonly record struct FlagsInfo {
    public readonly string Architecture;
    public readonly string EnumTypeName;
    public readonly int FlagCount;
    public readonly int Processor;

    public FlagsInfo(string architecture, string enumTypeName, int flagCount, int processor) {
        Architecture = architecture;
        EnumTypeName = enumTypeName;
        FlagCount = flagCount;
        Processor = processor;
    }
}