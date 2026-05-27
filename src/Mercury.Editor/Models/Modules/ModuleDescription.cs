using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Mercury.Editor.Models.Modules;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public abstract class ModuleDescription {

    /// <summary>
    /// Defines if the module is active and will be included in the simulation.
    /// </summary>
    [XmlAttribute("active")]
    public bool Active { get; set; } = true;
    
    public abstract string ModuleName { get; }

    public List<(PropertyInfo, string)> GetProperties() {
        List<(PropertyInfo, string)> list = GetSpecificProperties();
        return list;
    }

    protected abstract List<(PropertyInfo, string)> GetSpecificProperties();
}