using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Mercury.Editor.Generators.Modules;
using Mercury.Editor.Models.Modules.Properties;
using Mercury.Engine.Mips.Runtime.Simple;

namespace Mercury.Editor.Models.Modules;

/// <summary>
/// Represents the possible configurations for the <see cref="Monocycle"/> CPU module.
/// </summary>
[ModuleDescription]
public partial class MipsMonocycleModuleDescription : ModuleDescription, ICpuModuleDescription {
    
    public override string ModuleName => Localization.ModuleDescriptions.MipsMonoModuleNameValue;

    [XmlAttribute("useBranchDelaySlot")]
    [PropertyName(nameof(Localization.ModuleDescriptions.MipsMonoDelaySlotValue))]
    public bool UseBranchDelaySlot { get; set; } = true;
}