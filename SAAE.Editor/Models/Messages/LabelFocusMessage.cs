namespace SAAE.Editor.Models.Messages;

public class LabelFocusMessage
{
    public required string Name { get; init; }
    
    public required ulong Address { get; init; }
}