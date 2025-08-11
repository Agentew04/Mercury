using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models.Compilation;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.ViewModels;
using ProblemsViewModel = SAAE.Editor.ViewModels.Code.ProblemsViewModel;

namespace SAAE.Editor.Views.CodeView;

public partial class ProblemsView : UserControl
{
    public ProblemsView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<ProblemsViewModel>();
    }

    public ProblemsViewModel ViewModel { get; private set; }

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