using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
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
        InstructionViewModel vm = (InstructionViewModel)recipient;
        
        
    }
}

public partial class DisassemblyRow : ObservableObject {
     [ObservableProperty] private uint address;
     [ObservableProperty] private uint binary;
     [ObservableProperty] private string disassembly = string.Empty;
     [ObservableProperty] private SourceInstruction source = null!;
}

public partial class SourceInstruction : ObservableObject {
    [ObservableProperty] private bool generated;
    [ObservableProperty] private bool isPadding;
    [ObservableProperty] private string lineContent = string.Empty;
    [ObservableProperty] private int lineNumber;
    [ObservableProperty] private string file = string.Empty;
}