using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Services;
using SAAE.Engine.Mips.Runtime;

namespace SAAE.Editor.ViewModels.Execute;

public partial class RegisterViewModel : BaseViewModel
{
    // private readonly ExecuteService _executeService = App.Services.GetRequiredService<ExecuteService>();
    //
    // public RegisterViewModel()
    // {
    //     // _executeService.ProgramLoaded += OnProgramLoaded;
    // }
    //
    // private void OnProgramLoaded(object? sender, EventArgs e)
    // {
    //     machine = _executeService.GetCurrentMachine();
    //     Registers.Clear(); // limpa todos os registradores caso seja carregado um programa de
    //                        // uma arquitetura diferente
    //     
    //     Registers.AddRange(Enumerable.Range(0, (int)RegisterFile.Register.COUNT)
    //         .Select(x => {
    //             RegisterFile.Register i = (RegisterFile.Register)x;
    //             return new Register
    //             {
    //                 Name = i.ToString(),
    //                 Index = x,
    //                 Value = machine.Registers[x]
    //             };
    //         }
    //     ));
    //     machine.OnRegisterChanged += list =>
    //     {
    //         foreach (RegisterFile.Register reg in list)
    //         {
    //             int index = Registers.IndexOf(x => x.Index == (int)reg);
    //             if (index >= 0)
    //             {
    //                 Registers[index].Value = machine.Registers[reg];
    //             }
    //         }
    //     };
    // }
    //
    // private Machine machine;

    [ObservableProperty]
    private ObservableCollection<Register> registers;
}

public partial class Register : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private int index;

    [ObservableProperty] private int value;
}
