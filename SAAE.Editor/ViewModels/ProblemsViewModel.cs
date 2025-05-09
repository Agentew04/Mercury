using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Services;
using SAAE.Engine;

namespace SAAE.Editor.ViewModels;

public sealed partial class ProblemsViewModel : BaseViewModel
{
    private readonly CompilerService _compilerService = App.Services.GetRequiredKeyedService<CompilerService>(Architecture.Mips);

    public ProblemsViewModel()
    {
        
    }
    
    
    [RelayCommand]
    public void Compile()
    {
        
    }
}