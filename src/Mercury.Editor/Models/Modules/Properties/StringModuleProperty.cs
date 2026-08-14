using System;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Editor.Models.Modules.Properties;

public partial class StringModuleProperty : ModuleProperty {

    [ObservableProperty] private string value = string.Empty;
    
    public StringModuleProperty(string name, PropertyInfo info, ModuleDescription obj) : base(name, info, obj) {
        if (info.PropertyType != typeof(string)) {
            throw new ArgumentException($"Property {info.Name} must be of type string. Was: {info.PropertyType.Name}");
        }
    }

    public override void InitializeValues() {
        Value = (string)Info.GetValue(Obj)!;
    }

    partial void OnValueChanged(string value) {
        Info.SetValue(Obj, value);
    }
}