using Avalonia.Controls;
using Mercury.Editor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views;

public partial class AboutView : BaseWindow<AboutView,AboutViewModel> {
    public AboutView() {
        InitializeComponent();
    }
}