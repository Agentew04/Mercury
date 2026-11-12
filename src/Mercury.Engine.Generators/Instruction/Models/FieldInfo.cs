namespace Mercury.Generators.Instruction.Models;

internal readonly record struct FieldInfo(string FieldType, string FieldName, EquatableArray<FieldAttributeInfo> Attributes) {
    public readonly string FieldType = FieldType;
    public readonly string FieldName = FieldName;
    public readonly EquatableArray<FieldAttributeInfo> Attributes = Attributes;

    public bool IsSigned {
        get {
            foreach (FieldAttributeInfo attribute in Attributes) {
                if (attribute.Type == FieldAttributeType.Signed) {
                    return true;
                }
            }

            return false;
        }
    }
}