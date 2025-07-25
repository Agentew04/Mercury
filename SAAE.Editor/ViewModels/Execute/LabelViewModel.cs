using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SAAE.Editor.Models.Messages;

namespace SAAE.Editor.ViewModels.Execute;

public partial class LabelViewModel : BaseViewModel<LabelViewModel> {

    private List<Symbol> allSymbols = [];
    [ObservableProperty] private ObservableCollection<Symbol> symbols = [];
    [ObservableProperty] private int selectedTabIndex = 0;
    // 0 -> instrucoes
    // 1 -> data

    public LabelViewModel() {
        WeakReferenceMessenger.Default.Register<ProgramLoadMessage>(this, OnProgramLoad);
    }

    private static void OnProgramLoad(object recipient, ProgramLoadMessage msg) {
        LabelViewModel vm = (LabelViewModel)recipient;

        vm.allSymbols = msg.Metadata.Symbols
            .Where(x => !x.Name.StartsWith("__") && !x.Name.StartsWith("L.") && x.Name != "_gp")
            .ToList();
        vm.FilterSymbols();
    }

    partial void OnSelectedTabIndexChanged(int value) {
        FilterSymbols();
    }

    private void FilterSymbols() {
        Symbols.Clear();
        const uint textStart = 0x00400000;
        const uint dataStart = 0x10010000;
        foreach (Symbol symbol in allSymbols) {
            if (symbol.Address is >= textStart and < dataStart && SelectedTabIndex == 0) {
                // instrucao
                Symbols.Add(symbol);
            }
            else if(symbol.Address >= dataStart && SelectedTabIndex == 1){
                // data
                Symbols.Add(symbol);
            }
        }
    }
}