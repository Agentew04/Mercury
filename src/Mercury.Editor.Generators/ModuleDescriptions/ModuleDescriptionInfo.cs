using System.Collections.Immutable;

namespace Mercury.Generators.ModuleDescriptions;

internal readonly record struct ModuleDescriptionInfo(
        string Namespace,
        string ClassName,
        EquatableArray<ModulePropertyInfo> Properties) {

    public readonly string Namespace = Namespace;
    public readonly string ClassName = ClassName;
    public readonly EquatableArray<ModulePropertyInfo> Properties = Properties;
}