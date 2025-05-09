using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor.Views;

public partial class ProblemsView : UserControl
{
    public ProblemsView()
    {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<ProblemsViewModel>();
    }
    
    public ProblemsViewModel ViewModel { get; private set; }
}