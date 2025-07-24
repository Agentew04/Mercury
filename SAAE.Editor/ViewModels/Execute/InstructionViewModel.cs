using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SAAE.Editor.Models.Messages;
using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Instructions;

namespace SAAE.Editor.ViewModels.Execute;

public partial class InstructionViewModel : BaseViewModel<InstructionViewModel> {

    [ObservableProperty] private ObservableCollection<DisassemblyRow> instructions = [];
    [ObservableProperty] private int selectedInstructionIndex = -1;

    public InstructionViewModel() {
        WeakReferenceMessenger.Default.Register<ProgramLoadMessage>(this, OnProgramLoad);
    }

    #region Loading

    private static void OnProgramLoad(object recipient, ProgramLoadMessage msg) {
        InstructionViewModel vm = (InstructionViewModel)recipient;
        ProgramMetadata meta = msg.Metadata;

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
        
        Symbol previousSymbol = new("",0);
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
            previousSymbol = nextSymbol;
            hasSymbols = lineEnumerator.MoveNext();
            (nextSymbol, nextLine) = lineEnumerator.Current;
            address += 4;
        }
    }

    #endregion

    #region Execution

    // em milissegundos
    [ObservableProperty] private int executionSpeed = 100;

    [RelayCommand]
    private void Step() {
        
    }

    [RelayCommand]
    private void Execute() {
        
    }

    [RelayCommand]
    private void Stop() {
        
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
