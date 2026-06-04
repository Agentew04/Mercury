using System;
using System.Reflection;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Editor.Models.Modules.Properties;

public partial class IntegerModuleProperty : ModuleProperty {

    [ObservableProperty] private decimal minimum;
    [ObservableProperty] private decimal maximum;
    [ObservableProperty] private decimal? value;

    public IntegerModuleProperty(string name, PropertyInfo info, ModuleDescription obj) : base(name, info, obj) {
        Type type = Info.PropertyType;
        if (type != typeof(int) && type != typeof(uint) && type != typeof(long) && type != typeof(ulong)) {
            throw new ArgumentException($"Property {info.Name} isn't of supported numeric type: {type.Name}");
        }
    }

    public override void InitializeValues() {
        Type type = Info.PropertyType;
        
        if (type == typeof(int)) {
            Minimum = int.MinValue;
            Maximum = int.MaxValue;
            Value = (int)Info.GetValue(Obj)!;
        }else if (type == typeof(uint)) {
            Minimum = uint.MinValue;
            Maximum = uint.MaxValue;
            Value = (uint)Info.GetValue(Obj)!;
        }else if (type == typeof(long)) {
            Minimum = long.MinValue;
            Maximum = long.MaxValue;
            Value = (long)Info.GetValue(Obj)!;
        }else if (type == typeof(ulong)) {
            Minimum = ulong.MinValue;
            Maximum = ulong.MaxValue;
            Value = (ulong)Info.GetValue(Obj)!;
        }
    }

    partial void OnValueChanged(decimal? value) {
        decimal value1 = value ?? 0;
        Type type = Info.PropertyType;
        if (type == typeof(int)) {
            Info.SetValue(Obj, (int)value1);
        }else if (type == typeof(uint)) {
            Info.SetValue(Obj, (uint)value1);
        }else if (type == typeof(long)) {
            Info.SetValue(Obj, (long)value1);
        }else if (type == typeof(ulong)) {
            Info.SetValue(Obj, (ulong)value1);
        }
    }
}