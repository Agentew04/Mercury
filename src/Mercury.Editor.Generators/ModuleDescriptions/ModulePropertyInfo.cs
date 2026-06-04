namespace Mercury.Generators.ModuleDescriptions;

public readonly record struct ModulePropertyInfo(
        string Name,
        ModulePropertyType Type,
        string LocalizationKey) {
    public readonly string Name = Name;
    public readonly ModulePropertyType Type = Type;
    public readonly string LocalizationKey = LocalizationKey;
}