using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Mercury.Editor.Models;
using Mercury.Editor.Services;
using Mercury.Editor.Views;
using Microsoft.Extensions.DependencyInjection;
using Mercury.Editor.Extensions;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor;

public class App : Application {
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public static IServiceProvider Services { get; private set; } = null!;

    private static IClassicDesktopStyleApplicationLifetime? _desktopLifetime;
    public override async void OnFrameworkInitializationCompleted() {
        // pq essa linha de baixo estava aqui?? esperar algo quebrar pra voltar com ela
        //BindingPlugins.DataValidators.RemoveAt(0);

        Services = new ServiceCollection()
            .Configure()
            .BuildServiceProvider();

        var themeService = Services.GetRequiredService<ThemeService>();
        var logger = Services.GetRequiredService<ILogger<App>>();
        Stopwatch sw = new();
        sw.Start();
        themeService.LoadThemes(Resources);
        sw.Stop();
        logger.LogInformation("Initialized Themes in {Elapsed}ms", sw.ElapsedMilliseconds);
        
            
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            _desktopLifetime = desktop;
            desktop.Exit += OnAppExit;
            var splash = Services.GetRequiredService<SplashScreen>();
            desktop.MainWindow = splash;
            splash.Show();

            try {
                sw.Start();
                await splash.ViewModel.InitializeAsync();
                sw.Stop();
                logger.LogInformation("Splash initialized in {Elapsed}ms", sw.ElapsedMilliseconds);
            }
            catch (TaskCanceledException) {
                splash.Close();
                logger.LogError("Splash Task was cancelled. Closing Application");
                return;
            }
                
            // inicializacao completa
            // abre janela de selecao de projeto 
            // se nao foi passado por argumentos
            string? asmProjArg = desktop.Args?.FirstOrDefault(x => x.EndsWith(".asmproj"));
            string? directoryArg = desktop.Args?.Where(Directory.Exists)
                .FirstOrDefault(x => {
                    IEnumerable<string> files = Directory.EnumerateFiles(x);
                    return files.Any(f => f.EndsWith(".asmproj"));
                });

            if (asmProjArg is not null) {
                var projectService = Services.GetRequiredService<ProjectService>();
                sw.Start();
                ProjectFile? project = await projectService.OpenProject(asmProjArg.ToFilePath());
                if(project is null){
                    asmProjArg = null;
                }
                else {
                    projectService.SetCurrentProject(project);
                }
                sw.Stop();
                logger.LogInformation("Project Loaded in {Elapsed}ms", sw.ElapsedMilliseconds);
            }
                
            if (directoryArg is not null && asmProjArg is null) {
                var projectService = Services.GetRequiredService<ProjectService>();
                IEnumerable<string> files = Directory.EnumerateFiles(directoryArg);
                string? file = files.FirstOrDefault(f => f.EndsWith(".asmproj"));
                if(file is null){
                    directoryArg = null;
                }
                else {
                    sw.Start();
                    ProjectFile? project = await projectService.OpenProject(file.ToFilePath());
                    if(project is null){
                        directoryArg = null;
                    }
                    else {
                        projectService.SetCurrentProject(project);
                    }
                    sw.Stop();
                    logger.LogInformation("Project loaded in {Elapsed}ms", sw.ElapsedMilliseconds);
                }
            }
                
            ProjectSelectionView? projectSelection = null;
            if (asmProjArg is null && directoryArg is null) {
                projectSelection = Services.GetRequiredService<ProjectSelectionView>();
                desktop.MainWindow = projectSelection;
                projectSelection.Show();
                splash.Close();
                await projectSelection.ViewModel.WaitForProjectSelection();
                if (projectSelection.ViewModel.Cancelled) {
                    //desktop.Shutdown();
                    logger.LogInformation("Project Selection was cancelled. Exiting application.");
                    return;
                }
            }

            var guideService = Services.GetRequiredService<GuideService>();
            await guideService.InitializeAsync();
            
            // finalmente inicializa IDE
            var main = new MainWindow();
            desktop.MainWindow = main;
            main.Show();
            splash.Close();
            projectSelection?.Close();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static void Shutdown() {
        _desktopLifetime?.Shutdown();
    }

    public static event Action? OnExit;
    
    private static void OnAppExit(object? sender, ControlledApplicationLifetimeExitEventArgs e) {
        OnExit?.Invoke();
        if(Services is IDisposable dispose) dispose.Dispose();
    }
}