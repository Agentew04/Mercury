using Avalonia.Controls;
using Mercury.Editor.Models.Compilation;
using Mercury.Editor.ViewModels.Code;

namespace Mercury.Editor.Views.CodeView;

public partial class ProblemsView : BaseControl<ProblemsView, ProblemsViewModel>
{
    public ProblemsView() {
        InitializeComponent();
    }

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