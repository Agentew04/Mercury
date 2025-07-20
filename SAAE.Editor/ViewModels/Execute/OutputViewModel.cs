using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Services;

namespace SAAE.Editor.ViewModels.Execute;

public partial class OutputViewModel : BaseViewModel<OutputViewModel> {

    private readonly ExecuteService executeService = App.Services.GetRequiredService<ExecuteService>();
    
    // implementacao ruim... achar algo melhor para isto.
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(ClearCommand))] private string text = string.Empty;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SendCommand))] private string inputText = string.Empty;

    public OutputViewModel() {
        WeakReferenceMessenger.Default.Register<ProgramLoadMessage>(this, OnProgramLoaded);
    }

    private static void OnProgramLoaded(object receiver, ProgramLoadMessage msg) {
        
    }
    
    
    [RelayCommand(CanExecute = nameof(CanSend))]
    private void Send() {
        
    }

    [RelayCommand(CanExecute = nameof(CanClear))]
    private void Clear() {
        
    }

    private bool CanSend() {
        return !string.IsNullOrEmpty(InputText);
    }

    private bool CanClear() {
        
    }
}