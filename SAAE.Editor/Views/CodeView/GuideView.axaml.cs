using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;
using GuideViewModel = SAAE.Editor.ViewModels.Code.GuideViewModel;

namespace SAAE.Editor.Views.CodeView;

public partial class GuideView : UserControl {
    public GuideView() {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<GuideViewModel>();
        DataContext = ViewModel;
    }
    
    public GuideViewModel ViewModel { get; private set; }
}