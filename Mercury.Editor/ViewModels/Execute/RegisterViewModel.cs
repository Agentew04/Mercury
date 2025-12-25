using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Mercury.Editor.Localization;
using Mercury.Editor.Models.Messages;
using Mercury.Editor.Views.ExecuteView;
using Mercury.Engine.Common;
using Microsoft.Extensions.Logging;
using Mercury.Editor.Extensions;

namespace Mercury.Editor.ViewModels.Execute;

public partial class RegisterViewModel : BaseViewModel<RegisterViewModel, RegisterView> {

    [ObservableProperty]
    private int selectedProcessorIndex;

    [ObservableProperty]
    private ObservableCollection<RegisterGroup> registerGroups = [];

    [ObservableProperty] private ObservableCollection<string> processorNames = [];

    private readonly List<RegisterReference> highlightedRegisters = [];

    private ArchitectureMetadata architectureMetadata;
    private IRegisterHelper registerHelper = null!;

    private Machine? machine;

    public RegisterViewModel()
    {
        WeakReferenceMessenger.Default.Register<RegisterViewModel,ProgramLoadMessage>(this, OnProgramLoaded);
    }

    private static void OnProgramLoaded(RegisterViewModel vm, ProgramLoadMessage msg) {
        if (vm.machine is not null) {
            vm.machine.OnRegisterChanged -= vm.OnRegisterChange;
        }
        vm.machine = msg.MipsMachine;
        vm.architectureMetadata = ArchitectureManager.GetArchitectureMetadata(msg.MipsMachine.Architecture);
        vm.ProcessorNames = new ObservableCollection<string>(
            vm.architectureMetadata.Processors
                .Select(x => x.Name)
                .ToList());
        vm.registerHelper = RegisterHelperProvider.ProvideHelper(msg.MipsMachine.Architecture);
        vm.LoadRegisters(vm.SelectedProcessorIndex);
        vm.machine.OnRegisterChanged += vm.OnRegisterChange;
        // vm.Logger.LogInformation("Initialized register view with {registers} and {processors}", 
        //     vm.Registers.Count, 
        //     vm.architectureMetadata.Processors.Length);
    }

    private (RegisterDefinition def, Processor proc)? GetRegisterDefinition(Type t, Enum e) {
        foreach (Processor proc in architectureMetadata.Processors) {
            foreach (RegisterDefinition def in proc.Registers) {
                if (def.Type == t
                    && Equals(def.Reference, e)) {
                    return (def, proc);
                }
            }
        }
        Logger.LogError("Could not find register definition for type {type} and enum {enum}", t, e);
        return null;
    }

    private void OnRegisterChange(ValueTuple<Type,int>[] regs, int regCount) {
        highlightedRegisters.Clear();
        int updated = 0;
        for(int i=0;i<regCount;i++) {
            (Type registerType, int registerReference) = regs[i];
            // linha malevola!!!! mas 
            Enum boxedEnum = (Enum)Enum.ToObject(registerType, registerReference);
            (RegisterDefinition def, Processor proc)? regDef = GetRegisterDefinition(registerType, boxedEnum);
            if (!regDef.HasValue) {
                continue;
            }
            
            RegisterReference regRef = new() {
                Definition = regDef.Value.def,
                Processor = regDef.Value.proc
            };
            highlightedRegisters.Add(regRef);

            if (!regDef.Value.proc.Equals(architectureMetadata.Processors[SelectedProcessorIndex])) {     
                continue;
            }

            Register? register = RegisterGroups.SelectMany(x => x.Registers)
                .FirstOrDefault(x => x.Definition == regDef.Value.def);
            if (register is null) {
                Logger.LogWarning("Could not find (probable) loaded register to update value. Name: {name}", regDef.Value.def.Name);
                continue;
            }
            register.Values = GetRegisterValues(regRef.Definition);
            updated++;
        }
        Logger.LogInformation("Updated value of {count}/{total} registers", updated, regCount);
        Highlight();
    }

    private void Highlight() {
        int highlighted = 0;
        foreach (RegisterGroup regGroup in RegisterGroups) {
            foreach (Register register in regGroup.Registers) {
                register.Highlighted = highlightedRegisters.Any(x => x.Definition == register.Definition
                    && x.Processor == architectureMetadata.Processors[SelectedProcessorIndex]);
                if(register.Highlighted) highlighted++;
            }
        }
        Logger.LogInformation("Highlighted {reg} registers", highlighted);
    }

    private void LoadRegisters(int processorTabIndex) {
        RegisterGroups.Clear();
        if (architectureMetadata is null) return;
        Processor proc = architectureMetadata.Processors[processorTabIndex];

        IEnumerable<IGrouping<Type, RegisterDefinition>> groups = proc.Registers.GroupBy(x => x.Type);
        foreach (IGrouping<Type, RegisterDefinition> regGroup in groups) {
            IEnumerable<Register> regs = regGroup.Select(x => new Register {
                Definition = x,
                Values = GetRegisterValues(x)
            });
            string group = regGroup.Key.Name;
            
            RegisterGroups.Add(new RegisterGroup() {
                GroupName = group,
                Registers = new ObservableCollection<Register>(regs)
            });
        }
        Logger.LogInformation("Loaded {groups} register groups, totaling {total} registers.", 
            RegisterGroups.Count, proc.Registers.Length);
        Highlight();
    }

    partial void OnSelectedProcessorIndexChanged(int value) {
        LoadRegisters(value);
        Highlight();
    }
    
    public RegisterValues GetRegisterValues(RegisterDefinition definition) {
        int regValue = machine!.Registers.Get(definition.Reference, definition.Type);
        Span<byte> r = stackalloc byte[4];
        _ = BitConverter.TryWriteBytes(r, regValue);
        string s = Encoding.ASCII.GetString(r);
        RegisterValues values = new() {
            Decimal = regValue.ToString(),
            Hex = "0x" + regValue.ToString("X8"),
            Ascii = s.Escape(),
            AsFloat = BitConverter.Int32BitsToSingle(regValue)
        };
        if (definition.Number != -1)
        {
            // get next register
            Enum? nextRegEnum = registerHelper.GetRegisterFromNumberX(definition.Number+1, definition.Type);
            if (nextRegEnum is not null)
            {
                int nextRegValue = machine.Registers.Get(nextRegEnum, definition.Type);
                long combined = ((long)regValue << 32) | (uint)nextRegValue;
                values.AsDouble = BitConverter.Int64BitsToDouble(combined);
            }
        }
        return values;
    }
}

public partial class RegisterGroup : ObservableObject {
    [ObservableProperty] private string groupName = string.Empty;
    [ObservableProperty] private ObservableCollection<Register> registers = [];
}

public class RegisterReference {
    public required RegisterDefinition Definition { get; init; }
    public required Processor Processor { get; init; }
} 

public partial class Register : ObservableObject {
    [ObservableProperty] private RegisterDefinition definition;

    public bool HasIndex => Definition.Number != -1;

    [ObservableProperty] private bool highlighted;

    [ObservableProperty] private RegisterValues values = null!;
}

public class RegisterValues
{
    public string Decimal { get; set; } = string.Empty;
    public string Hex { get; set; } = string.Empty;
    public string Ascii { get; set; } = string.Empty;
    public float AsFloat { get; set; }
    public double? AsDouble { get; set; }
}

public class RegisterValueDoubleConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is null) {
            return RegisterResources.NotAvailableValue;
        }

        double d = (double)value;
        string s = d.ToString(CultureInfo.CurrentCulture); 
        return s;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return BindingNotification.Null;
    }
}

public class RegisterNumberConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is null) {
            return "null";
        }

        int index = (int)value;
        return index == -1 ? string.Empty : index.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return BindingNotification.Null;
    }
}