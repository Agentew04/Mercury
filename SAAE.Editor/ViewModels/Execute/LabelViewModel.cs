using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Views.ExecuteView;

namespace SAAE.Editor.ViewModels.Execute;

public partial class LabelViewModel : BaseViewModel<LabelViewModel, LabelView> {

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

        vm.allSymbols = msg.Metadata.GetUserDefinedSymbols().ToList();
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

    public void OnSymbolClicked(Symbol symbol) {
        Logger.LogInformation("Click on symbol {name}", symbol.Name);
        WeakReferenceMessenger.Default.Send(new LabelFocusMessage() {
            Name = symbol.Name,
            Address = symbol.Address
        });
    }
}