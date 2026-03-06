using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Editor.Views.Design;

namespace Mercury.Editor.ViewModels.Design;

public partial class DesignViewModel : BaseViewModel<DesignViewModel,DesignView> {

    [ObservableProperty] private EditorViewModel editorViewModel;

    public DesignViewModel() {
        EditorViewModel = new EditorViewModel();
    }

    protected override void OnLoaded() {
        TopLevel? top = TopLevel.GetTopLevel(GetView());
        EditorViewModel.TopLevel = top;
        EditorViewModel.UseTopLevel();
    }
}
