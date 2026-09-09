using Mercury.Editor.Controls;
using Mercury.Editor.Views.ExecuteView;

namespace Mercury.Editor.ViewModels.Execute;

public class MonitorViewModel : BaseViewModel<MonitorViewModel, MonitorWindow> {

    public PixelScreen? Screen { get; set; }
}