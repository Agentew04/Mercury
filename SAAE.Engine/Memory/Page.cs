namespace SAAE.Engine.Memory;

internal class Page {
    public int Number { get; set; }
    public byte[] Data { get; set; }

    public bool IsDirty { get; set; } = false;

    public Page(ulong size, int number){
        Data = new byte[size];
        Number = number;
    }
}