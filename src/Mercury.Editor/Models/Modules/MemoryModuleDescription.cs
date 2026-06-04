using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Mercury.Editor.Generators.Modules;
using Mercury.Editor.Models.Modules.Properties;

namespace Mercury.Editor.Models.Modules;
/// <summary>
/// Represents the configuration of the memory module.
/// </summary>
[ModuleDescription]
public partial class MemoryModuleDescription : ModuleDescription {
    
    [XmlAttribute("blockCount")]
    [PropertyName(nameof(Localization.ModuleDescriptions.MemoryBlockCountValue))]
    public ulong BlockCount { get; set; }
    
    [XmlAttribute("blockSize")]
    [PropertyName(nameof(Localization.ModuleDescriptions.MemoryBlockSizeValue))]
    public ulong BlockSize { get; set; }
    
    public override string ModuleName => Localization.ModuleDescriptions.MemoryModuleNameValue;
}