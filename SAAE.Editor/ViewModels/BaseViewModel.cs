using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SAAE.Editor.ViewModels;

public class BaseViewModel<TViewModel,TView> : ObservableObject where TView: class{
    
    // TODO: estudar um codegen, partial e essa funcao para automaticamente chamar localizacao de elementos calculados
    // protected virtual void Localize(){}

    private ILogger<TViewModel>? logger;

    /// <summary>
    /// Gets the class internal logging instance.
    /// </summary>
    protected ILogger<TViewModel> Logger => logger ??= App.Services.GetRequiredService<ILogger<TViewModel>>();

    private WeakReference<TView> viewReference;

    protected TView? GetView() {
        return viewReference.TryGetTarget(out TView? target) ? target : null;
    }

    public void SetView(TView view) {
        viewReference = new WeakReference<TView>(view);
    }
}