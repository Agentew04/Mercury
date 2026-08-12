using Mercury.Editor.Models.Node.DesignTime;

namespace Mercury.Editor.Services.Nodes;

public record Diagnostic(DiagnosticType Type, DesignBlock[]? Blocks, IoItem[]? Items, Connection[]? Connections);

public enum DiagnosticType {
    ConnectionBetweenTwoSizes,
    MultiDrivenInput,
    DuplicatedBlockName,
    DuplicateInputName,
    DuplicateOutputName,
    InvalidBlockName,
    InvalidInputName,
    InvalidOutputName
}