using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Mercury.Editor.Models.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Mercury.Editor.Models.Messages;
using Mercury.Editor.ViewModels;
using Code_ProblemsViewModel = Mercury.Editor.ViewModels.Code.ProblemsViewModel;
using ProblemsViewModel = Mercury.Editor.ViewModels.Code.ProblemsViewModel;

namespace Mercury.Editor.Views.CodeView;

public partial class ProblemsView : UserControl
{
    public ProblemsView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<Code_ProblemsViewModel>();
    }

    public Code_ProblemsViewModel ViewModel { get; private set; }

    private void DataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            Diagnostic diag = (Diagnostic)(e.AddedItems[0] ?? -1);
            ViewModel.OnSelectedIndexChanged(ViewModel.Diagnostics.IndexOf(diag));
        }
        ProblemsDataGrid.SelectedIndex = -1;
    }
}