using Avalonia.Controls;
using Mercury.Editor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.Views;

public abstract class BaseWindow<TView,TViewModel> : Window where TViewModel : BaseViewModel<TViewModel, TView>
                                                            where TView : BaseWindow<TView,TViewModel> {

    /// <summary>
    /// Gets the class internal logging instance.
    /// </summary>
    protected ILogger<TView> Logger => field ??= App.Services.GetRequiredService<ILogger<TView>>();
    
    public TViewModel ViewModel { get; }

    protected BaseWindow() {
        DataContext = ViewModel = App.Services.GetRequiredService<TViewModel>();
        ViewModel.SetView((TView)this);
    }
}

public abstract class BaseControl<TView, TViewModel> : UserControl where TViewModel : BaseViewModel<TViewModel, TView>
    where TView : BaseControl<TView, TViewModel> {
    
    /// <summary>
    /// Gets the class internal logging instance.
    /// </summary>
    protected ILogger<TView> Logger => field ??= App.Services.GetRequiredService<ILogger<TView>>();
    
    public TViewModel ViewModel { get; }

    protected BaseControl() {
        DataContext = ViewModel = App.Services.GetRequiredService<TViewModel>();
        ViewModel.SetView((TView)this);
    }
}