using System.Reflection;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Editor.Models.Modules.Properties;

/// <summary>
/// Represents one property that is present in a <see cref="ModuleDescription"/>. 
/// </summary>
/// <remarks>
/// This is a base type, so one of the child/explicit types must be used.
/// E.g. <see cref="IntegerModuleProperty"/> and <see cref="BooleanModuleProperty"/>.
/// </remarks>
public abstract partial class ModuleProperty : ObservableObject {

    /// <summary>
    /// A reference to the <see cref="ModuleDescription"/> object that holds the actual values for
    /// the properties. 
    /// </summary>
    protected readonly ModuleDescription Obj;
    /// <summary>
    /// A reference to the C# property inside the <see cref="Obj"/> object. 
    /// </summary>
    protected readonly PropertyInfo Info;
    
    protected ModuleProperty(string name, PropertyInfo info, ModuleDescription obj) {
        this.name = name;
        Info = info;
        Obj = obj;
    }
    
    /// <summary>
    /// The localized name for this property.
    /// </summary>
    [ObservableProperty] private string name;
    
    /// <summary>
    /// Fetches the actual values of the properties from the base object. This is not tied to constructor because
    /// we can't know order of deserialization that <see cref="XmlSerializer"/> generates. So, this is called
    /// lazily when <see cref="ModuleDescription.Properties"/> is first accessed.
    /// </summary>
    public abstract void InitializeValues();
}