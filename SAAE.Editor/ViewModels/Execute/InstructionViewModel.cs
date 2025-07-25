using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SAAE.Editor.Extensions;
using SAAE.Editor.Localization;
using SAAE.Editor.Models.Messages;
using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Instructions;
using SAAE.Engine.Mips.Runtime;

namespace SAAE.Editor.ViewModels.Execute;

public partial class InstructionViewModel : BaseViewModel<InstructionViewModel>, IDisposable {

    [ObservableProperty] private ObservableCollection<DisassemblyRow> instructions = [];
    [ObservableProperty] private int selectedInstructionIndex = -1;
    
    public InstructionViewModel() {
        WeakReferenceMessenger.Default.Register<ProgramLoadMessage>(this, OnProgramLoad);
        LocalizationManager.CultureChanged += OnLocalize;
    }

    private void OnLocalize(CultureInfo _) {
        OnPropertyChanged(nameof(ExecuteSpeedTooltip));
    }

    public void Dispose() {
        LocalizationManager.CultureChanged -= OnLocalize;
    }

    public string ExecuteSpeedTooltip => string.Format(InstructionResources.ExecuteSpeedTooltipValue,
        /*I/s*/ExecutionSpeed.ToString("F1"),
        /*ms/I*/(1000.0f/ExecutionSpeed).ToString("F1")
    );

    #region Loading

    private static void OnProgramLoad(object recipient, ProgramLoadMessage msg) {
        InstructionViewModel vm = (InstructionViewModel)recipient;
        ProgramMetadata meta = msg.Metadata;
        vm.machine = msg.Machine;
        vm.StepCommand.NotifyCanExecuteChanged();
        vm.ExecuteCommand.NotifyCanExecuteChanged();
        vm.StopCommand.NotifyCanExecuteChanged();
        vm.IsExecuting = false;

        vm.Instructions.Clear();
        for (int i = 0; i < meta.Files.Count; i++) {
            uint start = meta.Files[i].StartAddress;
            uint end = i < meta.Files.Count-1 ? meta.Files[i + 1].StartAddress : msg.Machine.Cpu.DropoffAddress;
            vm.ProcessFile(meta, meta.Files[i], start, end, msg.Machine.Memory, msg.Machine.Cpu.InstructionFactory);
        }
    }

    private void ProcessFile(ProgramMetadata meta, ObjectFile file, uint startAddress, uint endAddress, 
        IMemory memory, InstructionFactory factory) {
        IEnumerable<(Symbol x, int)> lineLabels = meta.Symbols
            .Where(x => x.Name.StartsWith("L."))
            .Select(x => {
                int dot1Idx = x.Name.IndexOf('.');
                int dot2Idx = x.Name.LastIndexOf('.');
                return (x, int.Parse(x.Name[(dot1Idx + 1)..dot2Idx]), int.Parse(x.Name[(dot2Idx + 1)..]));
            })
            .Where(x => x.Item2 == file.Index)
            .Select(x => (x.x, x.Item3));
        using IEnumerator<(Symbol x, int)> lineEnumerator = lineLabels.GetEnumerator();

        // faz um array com todas as linhas
        string fileContent = File.ReadAllText(file.Path.ToString());
        string[] splittedLines = fileContent.ReplaceLineEndings().Split(Environment.NewLine);
        
        uint address = startAddress;
        
        // Symbol previousSymbol = new("",0);
        int previousLine = 0;
        
        bool hasSymbols = lineEnumerator.MoveNext();
        (Symbol nextSymbol, int nextLine) = lineEnumerator.Current;
        
        while (address < endAddress) {
            // pega instrucao atual
            // se eh invalida:
            //     emite e fala que eh padding
            // senao: 
            //     enquanto nao acha o simbolo atual
            //         emite e fala que eh gerada pelo compilador
            //     emite instrucao com link para linha

            uint instructionBinary = (uint)memory.ReadWord(address);
            Instruction? instruction;
            try {
                instruction = factory.Disassemble(instructionBinary);
            }
            catch (Exception) {
                instruction = null;
            }

            if (instruction is null) {
                Instructions.Add(new DisassemblyRow() {
                    Address = address,
                    Binary = instructionBinary,
                    Disassembly = "",
                    Source = new SourceInstruction() {
                        File = file.Path.FullFileName,
                        Type = InstructionType.Padding,
                        LineContent = "",
                        LineNumber = previousLine
                    }
                });
                address += 4;
                continue;
            }

            // enquanto ainda nao chegou
            while (address < nextSymbol.Address && address < endAddress) {
                // fala que eh do anterior gerado
                Instructions.Add(new DisassemblyRow() {
                    Address = address,
                    Binary = instructionBinary,
                    Disassembly = instruction?.ToString() ?? "",
                    Source = new SourceInstruction() {
                        File = file.Path.FullFileName,
                        Type = instruction is not null ? InstructionType.Generated : InstructionType.Padding,
                        LineContent = "",
                        LineNumber = -1
                    }
                });
                address += 4;
                instructionBinary = (uint)memory.ReadWord(address);
                try {
                    instruction = factory.Disassemble(instructionBinary);
                }
                catch (Exception) {
                    instruction = null;
                }
            }
            
            Instructions.Add(new DisassemblyRow() {
                Address = address,
                Binary = instructionBinary,
                Disassembly = instruction?.ToString() ?? "its joever",
                Source = new SourceInstruction() {
                    File = file.Path.FullFileName,
                    Type = !hasSymbols ? InstructionType.Generated : InstructionType.Mapped,
                    LineNumber = nextLine,
                    LineContent = nextLine == 0 ? "" : splittedLines[nextLine-1]
                }
            });
            previousLine = nextLine;
            // previousSymbol = nextSymbol;
            hasSymbols = lineEnumerator.MoveNext();
            (nextSymbol, nextLine) = lineEnumerator.Current;
            address += 4;
        }
    }

    #endregion

    #region Execution

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExecuteSpeedTooltip))]
    private float executionSpeed = 5; // 5 IPS
    private Machine? machine = null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
    [NotifyCanExecuteChangedFor(nameof(StepCommand))]
    private bool isExecuting = false;

    public bool IsExecutionFinished => machine?.IsClockingFinished() ?? false;

    private PeriodicTimer? executionTimer;
    private CancellationTokenSource executionCts;

    [RelayCommand(CanExecute = nameof(CanStep))]
    private void Step() {
        machine!.Clock();
        int pc = machine!.Registers[RegisterFile.Register.Pc];
        int index = Instructions.IndexOf(x => x.Address == pc);
        SelectedInstructionIndex = index;
        OnPropertyChanged(nameof(IsExecutionFinished));
        StepCommand.NotifyCanExecuteChanged();
        ExecuteCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
    }

    private bool CanStep() {
        return machine is not null && !IsExecuting && !IsExecutionFinished;
    }

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private void Execute() {
        IsExecuting = true;
        executionCts = new CancellationTokenSource();
        executionTimer ??= new PeriodicTimer(TimeSpan.FromMilliseconds(1000.0f / ExecutionSpeed));
        _ = ExecuteTask();
    }

    private bool CanExecute() {
        return machine is not null && !IsExecuting && !IsExecutionFinished;
    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop() {
        IsExecuting = false;
        executionCts.Cancel();
    }

    private bool CanStop() {
        return machine is not null && IsExecuting && IsExecutionFinished;
    }

    partial void OnExecutionSpeedChanged(float value) {
        float delay = 1000.0f / value;
        executionTimer?.Dispose();
        executionTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(delay));
    }

    private async Task ExecuteTask() {
        while (!executionCts.IsCancellationRequested
            && await executionTimer!.WaitForNextTickAsync()){
            Step();
            if (!IsExecutionFinished) continue;
            IsExecuting = false;
            await executionCts.CancelAsync();
            break;
        }
    }

    #endregion
}

public partial class DisassemblyRow : ObservableObject {
     [ObservableProperty] private uint address;
     [ObservableProperty] private uint binary;
     [ObservableProperty] private string disassembly = string.Empty;
     [ObservableProperty] private SourceInstruction source = null!;
}

public partial class SourceInstruction : ObservableObject {
    [ObservableProperty] private InstructionType type;
    [ObservableProperty] private string lineContent = string.Empty;
    [ObservableProperty] private int lineNumber;
    [ObservableProperty] private string file = string.Empty;
}

public enum InstructionType {
    Unknown,
    Mapped,
    Generated,
    Padding
}
