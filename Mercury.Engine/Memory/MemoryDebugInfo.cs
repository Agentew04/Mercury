namespace Mercury.Engine.Memory;

public class MemoryDebugInfo : ICloneable {

    public int PageUnloads { get; set; }
    
    public int PageLoads { get; set; }

    public object Clone() {
        return new MemoryDebugInfo() {
            PageUnloads = PageUnloads
        };
    }
}
