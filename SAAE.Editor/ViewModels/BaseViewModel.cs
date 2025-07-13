using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SAAE.Editor.ViewModels;

public class BaseViewModel<T> : ObservableObject{
    
    // TODO: estudar um codegen, partial e essa funcao para automaticamente chamar localizacao de elementos calculados
    // protected virtual void Localize(){}

    protected static ILogger<T> GetLogger() {
        return App.Services.GetRequiredService<ILogger<T>>();
    }
}