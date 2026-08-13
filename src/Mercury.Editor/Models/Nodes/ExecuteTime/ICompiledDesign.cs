using System;
using Mercury.Editor.Models.Nodes.DesignTime;

namespace Mercury.Editor.Models.Nodes.ExecuteTime;

public interface ICompiledDesign : IDisposable {
    
    public void Clock();

    public T GetInputValue<T>(DesignBlock block, IoItem item);
    public T GetOutputValue<T>(DesignBlock block, IoItem item);
}