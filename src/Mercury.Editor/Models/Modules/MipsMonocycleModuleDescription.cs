using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Mercury.Editor.Models.Modules;

public class MipsMonocycleModuleDescription : ModuleDescription {
    public override string ModuleName => Localization.ModuleDescriptions.MipsMonoModuleNameValue;

    [XmlAttribute("useBranchDelaySlot")]
    public bool UseBranchDelaySlot { get; set; } = true;
    
    protected override List<(PropertyInfo, string)> GetSpecificProperties() {
        Type t = typeof(GpuModuleDescription);
        PropertyInfo[] props = t.GetProperties();
        PropertyInfo delaySlot = props.First(x => x.Name == "UseBranchDelaySlot");
        return [
            (delaySlot, Localization.ModuleDescriptions.MipsMonoDelaySlotValue),
        ];
    }
}