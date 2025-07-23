using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SAAE.Editor.Models.Messages;

namespace SAAE.Editor.ViewModels.Execute;

public partial class InstructionViewModel : BaseViewModel<InstructionViewModel> {

    [ObservableProperty] private ObservableCollection<DisassemblyRow> instructions = [];
    [ObservableProperty] private int selectedInstructionIndex = -1;

    public InstructionViewModel() {
        WeakReferenceMessenger.Default.Register<ProgramLoadMessage>(this, OnProgramLoad);
        Instructions.Add(new DisassemblyRow() {
            Address = 0x0040_0000,
            Binary = 0x1234_5678,
            Disassembly = "add $t0, $t1, $t2",
            Source = new SourceInstruction() {
                File = "main.asm",
                IsPadding = false,
                Generated = false,
                LineContent = "add $t0, $t1, $t2",
                LineNumber = 5
            }
        });
        Instructions.Add(new DisassemblyRow() {
            Address = 0x0040_0004,
            Binary = 0x1234_5678,
            Disassembly = "add $t0, $t1, $t2",
            Source = new SourceInstruction() {
                File = "main.asm",
                IsPadding = false,
                Generated = true,
                LineContent = "add $t0, $t1, $t2",
                LineNumber = 10
            }
        });
        Instructions.Add(new DisassemblyRow() {
            Address = 0x0040_0008,
            Binary = 0x1234_5678,
            Disassembly = "add $t0, $t1, $t2",
            Source = new SourceInstruction() {
                File = "main.asm",
                IsPadding = true,
                Generated = false,
                LineContent = "add $t0, $t1, $t2",
                LineNumber = 15
            }
        });
        Instructions.Add(new DisassemblyRow() {
            Address = 0x0040_000C,
            Binary = 0x1234_5678,
            Disassembly = "add $t0, $t1, $t2",
            Source = new SourceInstruction() {
                File = "main.asm",
                IsPadding = true,
                Generated = true,
                LineContent = "add $t0, $t1, $t2",
                LineNumber = 20
            }
        });
    }

    private static void OnProgramLoad(object recipient, ProgramLoadMessage msg) {
        
    }
}

public class DisassemblyRow {
    public uint Address { get;set; }
    public uint Binary { get; set; }
    public string Disassembly { get; set; } = string.Empty;
    public required SourceInstruction Source { get; set; }
}

public class SourceInstruction {
    public bool Generated { get; set; }
    public bool IsPadding { get; set; }
    public string LineContent { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string File { get; set; } = string.Empty;
}