using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;
using ProblemsViewModel = SAAE.Editor.ViewModels.Code.ProblemsViewModel;

namespace SAAE.Editor.Views.CodeView;

public partial class ProblemsView : UserControl
{
    public ProblemsView()
    {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<ProblemsViewModel>();
    }
    
    public ProblemsViewModel ViewModel { get; private set; }
}