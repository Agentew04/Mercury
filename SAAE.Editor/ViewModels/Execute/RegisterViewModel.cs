using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Extensions;
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
        vm.machine = msg.MipsMachine;
        vm.architectureMetadata = ArchitectureManager.GetArchitectureMetadata(msg.MipsMachine.Architecture);
        vm.ProcessorNames = new ObservableCollection<string>(
            vm.architectureMetadata.Processors
                .Select(x => x.Name)
                .ToList());
        vm.registerHelper = RegisterHelperProvider.ProvideHelper(msg.MipsMachine.Architecture);
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
                Registers[index].Value = machine.Registers.Get(reg.Item2, reg.Item1);
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
                Value = machine.Registers.Get(reg2, proc.RegistersType)
            });
        }
        Highlight();
        Logger.LogInformation("Loaded {registers} registers", proc.Registers.Length);
    }

    partial void OnSelectedProcessorIndexChanged(int value) {
        LoadRegisters(value);
        Highlight();
    }
    
    public RegisterInfo GetRegisterInfo(Register register) {
        Processor proc = architectureMetadata.Processors[SelectedProcessorIndex];
        Enum regEnum = registerHelper.GetRegisterFromNameX(register.Name, proc.RegistersType);
        int regValue = machine.Registers.Get(regEnum, proc.RegistersType);
        RegisterInfo info = new() {
            Name = register.Name,
            Number = register.Index == -1 ? null : register.Index,
            Decimal = regValue.ToString(),
            Hex = "0x" + regValue.ToString("8X"),
            Ascii = ((char)(regValue & 0xFF)).ToString(),
            AsFloat = BitConverter.Int32BitsToSingle(regValue)
        };
        if (register.Index != -1)
        {
            // get next register
            Enum? nextRegEnum = registerHelper.GetRegisterFromNumberX(register.Index+1, proc.RegistersType);
            if (nextRegEnum is not null)
            {
                int nextRegValue = machine.Registers.Get(nextRegEnum, proc.RegistersType);
                long combined = ((long)regValue << 32) | (uint)nextRegValue;
                info.AsDouble = BitConverter.Int64BitsToDouble(combined);
            }
        }
        return info;
    }
}

public partial class Register : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private int index;

    [ObservableProperty] private int value;

    [ObservableProperty] private bool highlighted;
}

public class RegisterInfo
{
    public string Name { get; set; } = string.Empty;
    public int? Number { get; set; }
    public string Decimal { get; set; } = string.Empty;
    public string Hex { get; set; } = string.Empty;
    public string Ascii { get; set; } = string.Empty;
    public float AsFloat { get; set; }
    public double? AsDouble { get; set; }
}
