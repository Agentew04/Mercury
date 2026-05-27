using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Mercury.Editor.Models.Modules;

public class MemoryModuleDescription : ModuleDescription {
    
    [XmlAttribute("blockCount")]
    public ulong BlockCount { get; set; }
    
    [XmlAttribute("blockSize")]
    public ulong BlockSize { get; set; }
    
    public override string ModuleName => Localization.ModuleDescriptions.MemoryModuleNameValue;
    
    protected override List<(PropertyInfo, string)> GetSpecificProperties() {
        Type t = typeof(GpuModuleDescription);
        PropertyInfo[] props = t.GetProperties();
        PropertyInfo count = props.First(x => x.Name == "BlockCount");
        PropertyInfo size = props.First(x => x.Name == "BlockSize");
        return [
            (count, Localization.ModuleDescriptions.MemoryBlockCountValue),
            (size, Localization.ModuleDescriptions.MemoryBlockSizeValue),
        ];
    }
}