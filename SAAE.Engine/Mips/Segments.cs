namespace SAAE.Engine.Mips; 

/// <summary>
/// Enumeration of the MIPS program segments.
/// </summary>
public readonly struct Segment {

    public string Name { get; init; }

    public ulong Address { get; init; }

    private Segment(string name, ulong address) {
        Name = name;
        Address = address;
    }

    public static Segment Extern => new("Extern", 0x10000000);

    public static Segment Data => new("Data", 0x10010000);

    public static Segment Text => new("Text", 0x00400000);

    public static Segment KData => new("KData", 0x90000000);

    public override readonly bool Equals(object? obj) {
        return obj is Segment segment &&
               Address == segment.Address;
    }

    public override readonly int GetHashCode() {
        return HashCode.Combine(Name, Address);
    }

    public static bool operator ==(Segment left, Segment right) {
        return EqualityComparer<Segment>.Default.Equals(left, right);
    }

    public static bool operator !=(Segment left, Segment right) {
        return !(left == right);
    }
}
