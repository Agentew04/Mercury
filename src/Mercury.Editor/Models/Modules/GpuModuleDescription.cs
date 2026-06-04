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
/// Depicts a gpu module that will be installed in the final system.
/// </summary>
[ModuleDescription]
public partial class GpuModuleDescription : ModuleDescription {

    /// <summary>
    /// The base address where the framebuffer will be located in virtual memory.
    /// </summary>
    [XmlAttribute("framebufferAddress")]
    [PropertyName(nameof(Localization.ModuleDescriptions.GpuFramebufferAddressValue))]
    public ulong BaseAddress { get; set; }
    
    /// <summary>
    /// The width in pixels of the framebuffer.
    /// </summary>
    [XmlAttribute("width")]
    [PropertyName(nameof(Localization.ModuleDescriptions.GpuFramebufferWidthValue))]
    public uint Width { get; set; }
    
    /// <summary>
    /// The height in pixels of the framebuffer.
    /// </summary>
    [XmlAttribute("height")]
    [PropertyName(nameof(Localization.ModuleDescriptions.GpuFramebufferHeightValue))]
    public uint Height { get; set; }

    public override string ModuleName => Localization.ModuleDescriptions.GpuModuleNameValue;
}