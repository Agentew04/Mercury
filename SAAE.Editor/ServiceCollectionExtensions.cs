using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;
using SAAE.Editor.ViewModels;
using SAAE.Editor.Views;

namespace SAAE.Editor;

public static class ServiceCollectionExtensions {
    public static void AddCommonServices(this IServiceCollection collection) {
        collection.AddTransient<SplashScreenViewModel>();
        collection.AddSingleton<ICompilerService, MipsCompiler>();
        collection.AddSingleton<SettingsService>();
        collection.AddSingleton<GuideViewModel>();
        collection.AddSingleton<GuideService>();
        collection.AddSingleton<ProjectSelectionViewModel>();
        collection.AddSingleton<ProjectService>();
        collection.AddSingleton<ProjectViewModel>();
    }
}