using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Mercury.Editor.ViewModels;
using Code_GuideViewModel = Mercury.Editor.ViewModels.Code.GuideViewModel;
using GuideViewModel = Mercury.Editor.ViewModels.Code.GuideViewModel;

namespace Mercury.Editor.Views.CodeView;

public partial class GuideView : UserControl {
    public GuideView() {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<Code_GuideViewModel>();
        DataContext = ViewModel;
    }
    
    public Code_GuideViewModel ViewModel { get; private set; }
}