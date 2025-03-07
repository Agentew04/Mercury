using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;
using SAAE.Editor.ViewModels;
using SAAE.Editor.Views;

namespace SAAE.Editor;

public static class ServiceCollectionExtensions {
    public static void AddCommonServices(this IServiceCollection collection) {

        #region ViewModels

        collection.AddTransient<SplashScreenViewModel>();
        collection.AddSingleton<GuideViewModel>();
        collection.AddSingleton<ProjectSelectionViewModel>();
        collection.AddSingleton<ProjectViewModel>();
        collection.AddSingleton<FileEditorViewModel>();

        #endregion

        #region Services

        collection.AddSingleton<ICompilerService, MipsCompiler>();
        collection.AddSingleton<SettingsService>();
        collection.AddSingleton<GuideService>();
        collection.AddSingleton<ProjectService>();
        collection.AddSingleton<FileService>();
        
        #endregion
        
    }
}