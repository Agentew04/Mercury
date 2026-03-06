using CommunityToolkit.Mvvm.Input;

namespace Mercury.Editor.ViewModels.Design;

public class PendingConnectionViewModel {
    private readonly EditorViewModel editor;
    private ConnectorViewModel source;

    public PendingConnectionViewModel(EditorViewModel editor) {
        this.editor = editor;
        StartCommand = new RelayCommand<ConnectorViewModel>(src => source = src!);
        FinishCommand = new RelayCommand<ConnectorViewModel>(target => {
            if (target is not null) {
                editor.Connect(source, target);
            }
        });
    }
    
    public IRelayCommand<ConnectorViewModel> StartCommand { get; }
    public IRelayCommand<ConnectorViewModel> FinishCommand { get; }
}