using System;
using System.Reflection;
using Mercury.Editor.Models.Node.DesignTime;

namespace Mercury.Editor.Models.Node.ExecuteTime;

public interface ICompiledDesign : IDisposable {
    
    public void Clock();

    public T GetInputValue<T>(DesignBlock block, IoItem item);
    public T GetOutputValue<T>(DesignBlock block, IoItem item);
}