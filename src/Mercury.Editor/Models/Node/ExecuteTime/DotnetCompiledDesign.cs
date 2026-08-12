using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mercury.Editor.Models.Node.DesignTime;

namespace Mercury.Editor.Models.Node.ExecuteTime;

#pragma warning disable IL2026
#pragma warning disable IL2070
#pragma warning disable IL2072
#pragma warning disable IL2075

public class DotnetCompiledDesign : ICompiledDesign {

    private record RuntimeBlock(object Instance, FieldInfo InputField, FieldInfo OutputField);
    private readonly object instance;
    private readonly MethodInfo tick;
    private readonly Dictionary<DesignBlock, RuntimeBlock> blocks = [];
    private readonly BlockLoadContext ctx;
    
    public DotnetCompiledDesign(Design design, Assembly asm, BlockLoadContext ctx) {
        this.ctx = ctx;
        TypeInfo type = asm.DefinedTypes.First(x => x.DeclaredMethods.Any(y => y.Name == "Tick"));
        instance = Activator.CreateInstance(type) ?? throw new Exception("No constructor found");
        MethodInfo? tickMethod = type.GetMethod("Tick");
        tick = tickMethod ?? throw new Exception("No tick method found");

        foreach (DesignBlock block in design.Blocks) {
            FieldInfo blockField = type.GetField(block.Name, BindingFlags.Instance | BindingFlags.Public)
                                   ?? throw new Exception($"Field {block.Name} not found");
            Type blockType = blockField.FieldType;
            FieldInfo inputField = blockType.GetField("input", BindingFlags.Instance | BindingFlags.Public)
                                   ?? throw new Exception($"Field input on {blockType.Name} not found");
            FieldInfo outputField = blockType.GetField("output", BindingFlags.Instance | BindingFlags.Public)
                                    ?? throw new Exception($"Field output on {blockType.Name} not found");
            
            blocks[block] = new RuntimeBlock(blockField.GetValue(instance)!, inputField, outputField);
        }
    }
    
    public void Clock() => tick.Invoke(instance, []);

    public T GetInputValue<T>(DesignBlock block, IoItem item) {
        (object blockInstance, FieldInfo inputField, _) = blocks[block];
        object inputStruct = inputField.GetValue(blockInstance)!;
        return (T)inputField.FieldType.GetField(item.Name)!.GetValue(inputStruct)!;
    }

    public T GetOutputValue<T>(DesignBlock block, IoItem item) {
        (object blockInstance, _, FieldInfo outputField) = blocks[block];
        object outputStruct = outputField.GetValue(blockInstance)!;
        return (T)outputField.FieldType.GetField(item.Name)!.GetValue(outputStruct)!;
    }

    public void Dispose() {
        ctx.Unload();
    }
}

#pragma warning restore IL2026
#pragma warning restore IL2070
#pragma warning restore IL2072
#pragma warning restore IL2075
