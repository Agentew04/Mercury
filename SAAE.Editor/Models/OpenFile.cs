using CommunityToolkit.Mvvm.ComponentModel;

namespace SAAE.Editor.Models;

public partial class OpenFile : ObservableObject {

    [ObservableProperty] private string filename;

    [ObservableProperty] private bool isDirty;
    
    [ObservableProperty] private string content;
}