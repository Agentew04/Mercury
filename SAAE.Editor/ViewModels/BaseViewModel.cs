using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SAAE.Editor.ViewModels;

public class BaseViewModel<T> : ObservableObject{
    
    // TODO: estudar um codegen, partial e essa funcao para automaticamente chamar localizacao de elementos calculados
    // protected virtual void Localize(){}

    private ILogger<T>? logger;

    /// <summary>
    /// Gets the class internal logging instance.
    /// </summary>
    protected ILogger<T> Logger => logger ??= App.Services.GetRequiredService<ILogger<T>>();
}