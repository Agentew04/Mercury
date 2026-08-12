using Mercury.Editor.Models.Node.DesignTime;

namespace Mercury.Editor.Models.Node.ExecuteTime;

public class InterpretedDesign : ICompiledDesign {
    
    // DAG
    
    public void Clock() {
    }

    public T GetInputValue<T>(DesignBlock block, IoItem item) {
        return default;
    }

    public T GetOutputValue<T>(DesignBlock block, IoItem item) {
        return default;
    }
}