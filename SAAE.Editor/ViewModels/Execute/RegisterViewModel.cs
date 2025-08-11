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
using SAAE.Engine.Common;
using SAAE.Engine.Mips.Runtime;

namespace SAAE.Editor.ViewModels.Execute;

public partial class RegisterViewModel : BaseViewModel<RegisterViewModel> {

    private readonly ExecuteService executeService = App.Services.GetRequiredService<ExecuteService>();
    
    [ObservableProperty]
    private int selectedProcessorIndex;

    [ObservableProperty]
    private ObservableCollection<Register> registers = [];

    [ObservableProperty] private ObservableCollection<string> processorNames = [];

    [ObservableProperty] private int selectedRegisterIndex = -1;
    private int  lastChangedRegisterIndex;

    private ArchitectureMetadata architectureMetadata;

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
        vm.LoadRegisters(vm.SelectedProcessorIndex);
        vm.machine.OnRegisterChanged += vm.OnRegisterChange;
        vm.Logger.LogInformation("Initialized register view with {registers} and {processors}", 
            vm.Registers.Count, 
            vm.architectureMetadata.Processors.Length);
    }

    private void OnRegisterChange(List<(Type,Enum)> regs) {
        if (regs.Count == 0) {
            lastChangedRegisterIndex = -1;
            return;
        }
        foreach ((Type,Enum) reg in regs) {
            string registerName = RegisterHelper.GetRegisterName(reg.Item2);
            int index = Registers.IndexOf(x => x.Name == registerName);
            if (index >= 0) {
                Registers[index].Value = machine.Registers.Get(reg.Item2, reg.Item1);
            }
            lastChangedRegisterIndex = index;
        }
        Logger.LogInformation("Updated value of {count} registers", regs.Count);
        Highlight();
    }

    private void Highlight() {
        SelectedRegisterIndex = lastChangedRegisterIndex;
    }

    private void LoadRegisters(int processorIndex) {
        Registers.Clear();
        SelectedRegisterIndex = -1;
        Processor proc = architectureMetadata.Processors[processorIndex];
        foreach (RegisterDefinition reg in proc.Registers) {
            Enum reg2 = RegisterHelper.GetRegisterFromName(reg.Name, proc.RegistersType);
            Registers.Add(new Register {
                Name = reg.Name,
                Index = reg.Number,
                Value = machine.Registers.Get(reg2, proc.RegistersType)
            });
        }
        Logger.LogInformation("Loaded {registers} registers", proc.Registers.Length);
    }

    partial void OnSelectedProcessorIndexChanged(int value) {
        LoadRegisters(value);
        Highlight();
    }
}

public partial class Register : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private int index;

    [ObservableProperty] private int value;
}
