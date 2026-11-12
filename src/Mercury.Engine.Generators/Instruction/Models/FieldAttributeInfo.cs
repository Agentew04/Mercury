namespace Mercury.Generators.Instruction.Models;

internal readonly record struct FieldAttributeInfo {
    public readonly FieldAttributeType Type;

    public readonly bool IsWellDefined;
    public readonly int BitStart;
    public readonly int BitEnd;

    public readonly string? AfterFieldName;
    public readonly string? WhenMethodName;
    public readonly int? Size;
    public readonly string? SizeMethodName;

    public FieldAttributeInfo(FieldAttributeType type, int bitStart, int bitEnd) {
        Type = type;
        if (type == FieldAttributeType.Field) {
            IsWellDefined = true;
            BitStart = bitStart;
            BitEnd = bitEnd;
        }
    }

    public FieldAttributeInfo(FieldAttributeType type, string param) {
        Type = type;
        if (type == FieldAttributeType.After) {
            AfterFieldName = param;
        }else if (type == FieldAttributeType.When) {
            WhenMethodName = param;
        }else if (type == FieldAttributeType.Size) {
            SizeMethodName = param;
        }
    }

    public FieldAttributeInfo(FieldAttributeType type, int size) {
        Type = type;
        if (type == FieldAttributeType.Size) {
            Size = size;
        }
    }

    public FieldAttributeInfo(FieldAttributeType type) {
        Type = type;
    }
    
    
}

internal enum FieldAttributeType {
    Field,
    After,
    When,
    Size,
    Signed
}
