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