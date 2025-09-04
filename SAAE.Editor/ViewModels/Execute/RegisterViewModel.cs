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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Extensions;
using SAAE.Editor.Localization;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;
using SAAE.Editor.Views.ExecuteView;
using SAAE.Engine.Common;
using SAAE.Engine.Mips.Runtime;

namespace SAAE.Editor.ViewModels.Execute;

public partial class RegisterViewModel : BaseViewModel<RegisterViewModel, RegisterView> {

    [ObservableProperty]
    private int selectedProcessorIndex;

    [ObservableProperty]
    private ObservableCollection<Register> registers = [];

    [ObservableProperty] private ObservableCollection<string> processorNames = [];

    private readonly List<(Type, Enum)> highlightedRegisters = [];

    private ArchitectureMetadata architectureMetadata;
    private IRegisterHelper registerHelper = null!;

    private Machine machine = null!;

    public RegisterViewModel()
    {
        WeakReferenceMessenger.Default.Register<RegisterViewModel,ProgramLoadMessage>(this, OnProgramLoaded);
    }

    private static void OnProgramLoaded(RegisterViewModel vm, ProgramLoadMessage msg) {
        if (vm.machine is not null) {
            vm.machine.OnRegisterChanged -= vm.OnRegisterChange;
        }
        vm.machine = msg.Machine;
        vm.architectureMetadata = ArchitectureManager.GetArchitectureMetadata(msg.Machine.Architecture);
        vm.ProcessorNames = new ObservableCollection<string>(
            vm.architectureMetadata.Processors
                .Select(x => x.Name)
                .ToList());
        vm.registerHelper = RegisterHelperProvider.ProvideHelper(msg.Machine.Architecture);
        vm.LoadRegisters(vm.SelectedProcessorIndex);
        vm.machine.OnRegisterChanged += vm.OnRegisterChange;
        vm.Logger.LogInformation("Initialized register view with {registers} and {processors}", 
            vm.Registers.Count, 
            vm.architectureMetadata.Processors.Length);
    }

    private void OnRegisterChange(List<(Type,Enum)> regs) {
        highlightedRegisters.Clear();
        foreach ((Type,Enum) reg in regs) {
            string registerName = registerHelper.GetRegisterNameX(reg.Item2);
            int index = Registers.IndexOf(x => x.Name == registerName);
            if (index >= 0) {
                Registers[index].Values = GetRegisterValues(registerName, index);
            }
            highlightedRegisters.Add(reg);
        }
        Logger.LogInformation("Updated value of {count} registers", regs.Count);
        Highlight();
    }

    private void Highlight() {
        foreach (var register in Registers) {
            register.Highlighted = false;
        }
        Type currentType = architectureMetadata.Processors[SelectedProcessorIndex].RegistersType;
        foreach ((Type, Enum) highlight in highlightedRegisters) {
            if (highlight.Item1 != currentType) {
                continue;
            }
            string registerName = registerHelper.GetRegisterNameX(highlight.Item2);
            Register? register = Registers.FirstOrDefault(x => x.Name == registerName);
            if (register is null) {
                continue;
            }
            register.Highlighted = true;
        }
    }

    private void LoadRegisters(int processorIndex) {
        Registers.Clear();
        Processor proc = architectureMetadata.Processors[processorIndex];
        foreach (RegisterDefinition reg in proc.Registers) {
            Enum reg2 = registerHelper.GetRegisterFromNameX(reg.Name, proc.RegistersType);
            Registers.Add(new Register {
                Name = reg.Name,
                Index = reg.Number,
                Values = GetRegisterValues(reg.Name, reg.Number)
            });
        }
        Highlight();
        Logger.LogInformation("Loaded {registers} registers", proc.Registers.Length);
    }

    partial void OnSelectedProcessorIndexChanged(int value) {
        LoadRegisters(value);
        Highlight();
    }
    
    public RegisterValues GetRegisterValues(string name, int index) {
        Processor proc = architectureMetadata.Processors[SelectedProcessorIndex];
        Enum? regEnum = registerHelper.GetRegisterFromNameX(name, proc.RegistersType);
        if (regEnum is null) {
            foreach (Processor processor in architectureMetadata.Processors) {
                regEnum = registerHelper.GetRegisterFromNameX(name, processor.RegistersType);
                if (regEnum is not null) {
                    break;
                }
            }

            if (regEnum is null) {
                Logger.LogError("Could not find any register with id: {name}/{index}", name, index);
                return new RegisterValues();
            }
        }
        int regValue = machine.Registers.Get(regEnum, proc.RegistersType);
        Span<byte> r = stackalloc byte[4];
        _ = BitConverter.TryWriteBytes(r, regValue);
        string s = Encoding.ASCII.GetString(r);
        RegisterValues values = new() {
            Decimal = regValue.ToString(),
            Hex = "0x" + regValue.ToString("X8"),
            Ascii = s.Escape(),
            AsFloat = BitConverter.Int32BitsToSingle(regValue)
        };
        if (index != -1)
        {
            // get next register
            Enum? nextRegEnum = registerHelper.GetRegisterFromNumberX(index+1, proc.RegistersType);
            if (nextRegEnum is not null)
            {
                int nextRegValue = machine.Registers.Get(nextRegEnum, proc.RegistersType);
                long combined = ((long)regValue << 32) | (uint)nextRegValue;
                values.AsDouble = BitConverter.Int64BitsToDouble(combined);
            }
        }
        return values;
    }
}

public partial class Register : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(HasIndex))]
    private int index;

    public bool HasIndex => Index != -1;

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
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is null) {
            return RegisterResources.NotAvailableValue;
        }

        double d = (double)value;
        string s = d.ToString(CultureInfo.CurrentCulture); 
        return s;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return BindingNotification.Null;
    }
}

public class RegisterNumberConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is null) {
            return "null";
        }

        int index = (int)value;
        return index == -1 ? string.Empty : index.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return BindingNotification.Null;
    }
}