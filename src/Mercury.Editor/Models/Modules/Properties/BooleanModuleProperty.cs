using System;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Editor.Models.Modules.Properties;

public partial class BooleanModuleProperty : ModuleProperty {

    [ObservableProperty] private bool value;
    
    public BooleanModuleProperty(string name, PropertyInfo info, ModuleDescription obj) : base(name, info, obj) {
        if (info.PropertyType != typeof(bool)) {
            throw new ArgumentException($"Property {info.Name} must be of type boolean. Was: {info.PropertyType.Name}");
        }
    }

    public override void InitializeValues() {
        Value = (bool)Info.GetValue(Obj)!;
    }

    partial void OnValueChanged(bool value) {
        Info.SetValue(Obj, value);
    }
}