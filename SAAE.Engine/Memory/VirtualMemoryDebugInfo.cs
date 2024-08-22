namespace SAAE.Engine.Memory;

public class VirtualMemoryDebugInfo : ICloneable {

    public int PageUnloads { get; set; }
    
    public int PageLoads { get; set; }

    public object Clone() {
        return new VirtualMemoryDebugInfo() {
            PageUnloads = PageUnloads
        };
    }
}
