using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Models.Compilation;
using SAAE.Editor.Services;
using SAAE.Engine;

namespace SAAE.Editor.ViewModels;

public sealed partial class ProblemsViewModel : BaseViewModel
{
    private readonly ICompilerService _compilerService = App.Services.GetRequiredKeyedService<ICompilerService>(Architecture.Mips);
    private readonly ProjectService _projectService = App.Services.GetRequiredService<ProjectService>();

    public ProblemsViewModel()
    {
        
    }

    [ObservableProperty] 
    private string output = "";

    [ObservableProperty] 
    private ObservableCollection<Diagnostic> diagnostics = [];
    
    [RelayCommand]
    private async Task Compile()
    {
        ProjectFile? project = _projectService.GetCurrentProject();
        if(project is null) return;
        string path = Path.Combine(project.ProjectDirectory, project.EntryFile);
        Console.WriteLine("Compilando " + path);
        await using Stream stream = File.OpenRead(path);
        CompilationResult result = await _compilerService.CompileStandaloneAsync(stream);

        if (!result.IsSuccess) {
            if (result.ErrorStream is null) {
                Console.WriteLine("nao tinha error stream");
                return;
            }
            using StreamReader sr = new(result.ErrorStream);
            Output = await sr.ReadToEndAsync();
            Console.WriteLine("tinha error stream");
        }
        else {
            Console.WriteLine("Compilado com sucesso");
            result.OutputElf?.Dispose();
            if (result.OutputStream != null) await result.OutputStream.DisposeAsync();
        }
        
        Diagnostics.Clear();
        if (result.Diagnostics is not null) {
            Diagnostics.AddRange(result.Diagnostics);
        }
    }
}