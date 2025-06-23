using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
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
    public static void AddCommonServices(this IServiceCollection collection) {

        #region ViewModels

        collection.AddTransient<SplashScreenViewModel>();
        collection.AddSingleton<GuideViewModel>();
        collection.AddSingleton<ProjectSelectionViewModel>();
        collection.AddSingleton<ProjectViewModel>();
        collection.AddSingleton<FileEditorViewModel>();
        collection.AddSingleton<ProblemsViewModel>();
        collection.AddSingleton<RegisterViewModel>();

        #endregion

        #region Services

        collection.AddKeyedSingleton<ICompilerService, MipsCompiler>(Architecture.Mips);
        collection.AddSingleton<SettingsService>();
        collection.AddSingleton<GuideService>();
        collection.AddSingleton<ProjectService>();
        collection.AddSingleton<FileService>();
        collection.AddSingleton<GrammarService>();
        collection.AddSingleton<ExecuteService>();

        HttpClient httpClient = new(); // reuse the same instance
        HttpRequestHeaders headers = httpClient.DefaultRequestHeaders;
        headers.UserAgent.ParseAdd("SAAE/" + typeof(App).Assembly.GetName().Version);
        collection.AddSingleton(httpClient);
        
        #endregion

    }
}