using Microsoft.CodeAnalysis;

namespace Mercury.Generators.Instruction;

internal static class InstructionDiagnostics {
    
    public static readonly DiagnosticDescriptor ImplementInterface = new(
        id: "MRCY0001",
        title: "Implement IInstruction Interface",
        messageFormat: "Instructions must implement the IInstruction interface",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UsePartial = new(
        id: "MRCY0002",
        title: "Mark class as partial",
        messageFormat: "Instruction classes must be marked as partial for the generator to create the correct methods",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FieldNoAttribute = new(
        id: "MRCY0003",
        title: "Add [Instruction] attribute",
        messageFormat: "Members with [Field] attribute must be placed on a class that has the [Instruction] attribute",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InsufficientFieldSize = new(
        id: "MRCY0004",
        title: "Variable type is too small",
        messageFormat: "The type of the variable does not fit all possible data in the instruction field, consider increasing it",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FormattingAmbiguity = new(
        id: "MRCY0005",
        title: "Instruction format is ambiguous",
        messageFormat: "Fields and Formats must cover all 32 bits space of the binary representation: coverage hex {0}",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

}