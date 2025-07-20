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

    private ArchitectureMetadata architectureMetadata;

    private Machine machine = null!;

    public RegisterViewModel()
    {
        WeakReferenceMessenger.Default.Register<ProgramLoadMessage>(this, OnProgramLoaded);
    }

    private void OnProgramLoaded(object? sender, ProgramLoadMessage msg) {
        machine = msg.Machine;
        architectureMetadata = ArchitectureManager.GetArchitectureMetadata(msg.Machine.Architecture);
        ProcessorNames = new ObservableCollection<string>(
            architectureMetadata.Processors
                .Select(x => x.Name)
                .ToList());
        LoadRegisters(SelectedProcessorIndex);
        machine.OnRegisterChanged += list =>
        {
            foreach (RegisterFile.Register reg in list) {
                int index = Registers.IndexOf(x => x.Index == (int)reg);
                if (index >= 0)
                {
                    Registers[index].Value = machine.Registers[reg];
                }
            }
            Logger.LogInformation("Updated value of {count} registers", list.Count);
        };
        Logger.LogInformation("Initialized register view with {registers} and {processors}", Registers.Count, architectureMetadata.Processors.Length);
    }

    private void LoadRegisters(int processorIndex) {
        Registers.Clear();

        Processor proc = architectureMetadata.Processors[processorIndex];
        foreach (RegisterDefinition reg in proc.Registers) {
            // TODO: enum com hierarquia?
            // isto esta hardcoded para strings e mips
            int number = reg.Number != -1 ? reg.Number : (reg.Name == "pc" ? 32 : reg.Name == "hi" ? 33 : 34);
            Registers.Add(new Register {
                Name = reg.Name,
                Index = number,
                Value = machine.Registers.Get(number)
            });
        }
        Logger.LogInformation("Loaded {registers} registers", proc.Registers.Length);
    }

    partial void OnSelectedProcessorIndexChanged(int value) {
        LoadRegisters(value);
    }
}

public partial class Register : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private int index;

    [ObservableProperty] private int value;
}
