using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Editor.Models.Modules.Properties;
using Mercury.Editor.Utils;

namespace Mercury.Editor.Models.Modules;

/// <summary>
/// Represents a base module description. Implements shared behaviour between child classes.
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public abstract partial class ModuleDescription : ObservableObject {

    /// <summary>
    /// Defines if the module is active and will be included in the simulation.
    /// </summary>
    [ObservableProperty]
    [property: XmlAttribute("active")]
    private bool active = true;
    
    [XmlIgnore]
    public abstract string ModuleName { get; }

    [XmlIgnore]
    public ObservableCollectionEx<ModuleProperty> Properties
    {
        get {
            if (field != null) {
                return field;
            }
            // fetch properties. Values should be initialized by now
            field = [];
            field.AddRange(GetSpecificProperties());
            foreach (ModuleProperty property in field) {
                property.InitializeValues();
            }
            field.ItemPropertyChanged += (s, e) => {
                OnPropertyChanged();
            };
            OnPropertyChanged();

            return field;
        }
    } = null;

    /// <summary>
    /// Returns a list of editable properties that this module should expose to the end user.
    /// </summary>
    protected abstract List<ModuleProperty> GetSpecificProperties();
}