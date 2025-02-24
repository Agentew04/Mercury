using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;
using SAAE.Editor.Views;

namespace SAAE.Editor {
    public class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public static ServiceProvider Services { get; private set; } = null!;
        
        public override async void OnFrameworkInitializationCompleted() {
            BindingPlugins.DataValidators.RemoveAt(0);
            
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddCommonServices();

            Services = serviceCollection.BuildServiceProvider();
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.Exit += OnAppExit;
                var splash = new SplashScreen();
                desktop.MainWindow = splash;
                splash.Show();

                try {
                    await splash.ViewModel.InitializeAsync();
                }
                catch (TaskCanceledException) {
                    splash.Close();
                    return;
                }
                
                // inicializacao completa
                // abre janela de selecao de projeto 
                // se nao foi passado por argumentos
                string? asmProjArg = desktop.Args?.FirstOrDefault(x => x.EndsWith(".asmproj"));
                string? directoryArg = desktop.Args?.Where(Directory.Exists)
                    .Where(x => {
                        IEnumerable<string> files = Directory.EnumerateFiles(x);
                        return files.Any(f => f.EndsWith(".asmproj"));
                    })
                    .FirstOrDefault();

                if (asmProjArg is not null) {
                    var projectService = Services.GetRequiredService<ProjectService>();
                    ProjectFile? project = await projectService.OpenProject(asmProjArg);
                    if(project is null){
                        asmProjArg = null;
                    }
                    else {
                        projectService.SetCurrentProject(project);
                    }
                }
                
                if (directoryArg is not null && asmProjArg is null) {
                    var projectService = Services.GetRequiredService<ProjectService>();
                    IEnumerable<string> files = Directory.EnumerateFiles(directoryArg);
                    string? file = files.FirstOrDefault(f => f.EndsWith(".asmproj"));
                    if(file is null){
                        directoryArg = null;
                    }
                    else {
                        ProjectFile? project = await projectService.OpenProject(file);
                        if(project is null){
                            directoryArg = null;
                        }
                        else {
                            projectService.SetCurrentProject(project);
                        }
                    }
                }
                
                ProjectSelectionView? projectSelection = null;
                if (asmProjArg is null && directoryArg is null) {
                    projectSelection = new ProjectSelectionView();
                    desktop.MainWindow = projectSelection;
                    projectSelection.Show();
                    splash.Close();
                    await projectSelection.SelectionViewModel.WaitForProjectSelection();
                    if (projectSelection.SelectionViewModel.Cancelled) {
                        //desktop.Shutdown();
                        return;
                    }
                }
                
                // finalmente inicializa IDE
                var main = new MainWindow();
                desktop.MainWindow = main;
                main.Show();
                projectSelection?.Close();
            }

            base.OnFrameworkInitializationCompleted();
        }
        
        private static void OnAppExit(object? sender, ControlledApplicationLifetimeExitEventArgs e) {
            Services.Dispose();
        }
    }
}