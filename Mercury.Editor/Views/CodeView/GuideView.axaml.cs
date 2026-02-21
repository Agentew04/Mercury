using Mercury.Editor.ViewModels.Code;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views.CodeView;

public partial class GuideView : BaseControl<GuideView, GuideViewModel> {
    public GuideView() {
        InitializeComponent();
    }
}