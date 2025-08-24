using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using SAAE.Editor.Models;
using SAAE.Editor.Services;
using SAAE.Editor.ViewModels;
using SAAE.Editor.ViewModels.Code;
using SAAE.Editor.ViewModels.Execute;
using SAAE.Editor.Views;
using SAAE.Engine;
using FileEditorViewModel = SAAE.Editor.ViewModels.Code.FileEditorViewModel;
using GuideViewModel = SAAE.Editor.ViewModels.Code.GuideViewModel;
using ProblemsViewModel = SAAE.Editor.ViewModels.Code.ProblemsViewModel;
using ProjectViewModel = SAAE.Editor.ViewModels.Code.ProjectViewModel;

namespace SAAE.Editor;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddCommonServices(this IServiceCollection collection) {

        #region ViewModels

        collection.AddTransient<SplashScreenViewModel>();
        collection.AddSingleton<GuideViewModel>();
        collection.AddSingleton<ProjectSelectionViewModel>();
        collection.AddSingleton<ProjectViewModel>();
        collection.AddSingleton<FileEditorViewModel>();
        collection.AddSingleton<ProblemsViewModel>();
        collection.AddSingleton<RegisterViewModel>();
        collection.AddTransient<ProjectConfigurationViewModel>(); // transient pq nos deletamos a window ao aplicar
        collection.AddSingleton<OutputViewModel>();
        collection.AddSingleton<RamViewModel>();
        collection.AddSingleton<InstructionViewModel>();
        collection.AddSingleton<LabelViewModel>();

        #endregion

        #region Services

        collection.AddKeyedSingleton<ICompilerService, MipsCompiler>(Architecture.Mips);
        collection.AddSingleton<SettingsService>();
        collection.AddSingleton<GuideService>();
        collection.AddSingleton<ProjectService>();
        collection.AddSingleton<FileService>();
        collection.AddSingleton<GrammarService>();
        collection.AddSingleton<ExecuteService>();
        collection.AddSingleton<UpdaterService>();

        HttpClient httpClient = new(); // reuse the same instance
        HttpRequestHeaders headers = httpClient.DefaultRequestHeaders;
        headers.UserAgent.ParseAdd("MercuryIDE/" + typeof(App).Assembly.GetName().Version);
        collection.AddSingleton(httpClient);
        
        #endregion

        return collection;
    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection collection)
    {
        collection.AddLogging(logBuilder =>
        {
            logBuilder.AddConsole();
#if DEBUG
            logBuilder.SetMinimumLevel(LogLevel.Debug);
#elif RELEASE
            logBuilder.SetMinimumLevel(LogLevel.Error);
#endif
        });

        return collection;
    }
}