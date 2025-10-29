using Mercury.Editor.Extensions;

namespace Mercury.Editor.Models.Compilation;

public record struct Diagnostic {
    
    public PathObject FilePath { get; set; }
    
    public int Line { get; set; }
    
    public int Column { get; set; }
    
    public DiagnosticType Type { get; set; }
    
    public string Message { get; set; }
}

public enum DiagnosticType {
    Unknown,
    Information,
    Warning,
    Error,
}