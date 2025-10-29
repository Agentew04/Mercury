using System.Net.Http;
using System.Net.Http.Headers;
using Mercury.Editor.Services;
using Mercury.Editor.ViewModels;
using Mercury.Editor.ViewModels.Execute;
using Mercury.Engine.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Mercury.Editor.Models;
using Mercury.Editor.ViewModels.Code;
using Mercury.Editor.Views;
using Mercury.Engine;
using FileEditorViewModel = Mercury.Editor.ViewModels.Code.FileEditorViewModel;
using GuideViewModel = Mercury.Editor.ViewModels.Code.GuideViewModel;
using ProblemsViewModel = Mercury.Editor.ViewModels.Code.ProblemsViewModel;
using ProjectViewModel = Mercury.Editor.ViewModels.Code.ProjectViewModel;

namespace Mercury.Editor;

using FileEditorViewModel = ViewModels.Code.FileEditorViewModel;
using GuideViewModel = ViewModels.Code.GuideViewModel;
using ProblemsViewModel = ViewModels.Code.ProblemsViewModel;
using ProjectViewModel = ViewModels.Code.ProjectViewModel;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddCommonServices(this IServiceCollection collection) {

        #region ViewModels

        // singleton
        collection.AddSingleton<GuideViewModel>();
        collection.AddSingleton<ProjectSelectionViewModel>();
        collection.AddSingleton<ProjectViewModel>();
        collection.AddSingleton<FileEditorViewModel>();
        collection.AddSingleton<ProblemsViewModel>();
        collection.AddSingleton<RegisterViewModel>();
        collection.AddSingleton<OutputViewModel>();
        collection.AddSingleton<RamViewModel>();
        collection.AddSingleton<InstructionViewModel>();
        collection.AddSingleton<LabelViewModel>();
        
        // transient
        collection.AddTransient<SplashScreenViewModel>();
        collection.AddTransient<ProjectConfigurationViewModel>();
        collection.AddTransient<AboutViewModel>(); 
        collection.AddTransient<PreferencesViewModel>();

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