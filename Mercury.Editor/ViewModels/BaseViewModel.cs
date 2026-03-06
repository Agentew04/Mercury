using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.ViewModels;

public abstract class BaseViewModel<TViewModel,TView> : ObservableObject where TView : Control {
    /// <summary>
    /// Gets the class internal logging instance.
    /// </summary>
    protected ILogger<TViewModel> Logger => field ??= App.Services.GetRequiredService<ILogger<TViewModel>>();

    private WeakReference<TView>? viewReference;

    protected TView? GetView() {
        if (viewReference is null) return null;
        return viewReference.TryGetTarget(out TView? target) ? target : null;
    }

    public void SetView(TView view) {
        viewReference = new WeakReference<TView>(view);
        if (view.IsLoaded) {
            OnLoaded();
        }
        else {
            view.Loaded += (_,_) => OnLoaded();
        }
    }

    protected virtual void OnLoaded(){}
}