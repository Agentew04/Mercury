using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;
using SAAE.Editor.ViewModels;

namespace SAAE.Editor;

public static class ServiceCollectionExtensions {
    public static void AddCommonServices(this IServiceCollection collection) {
        collection.AddTransient<SplashScreenViewModel>();
        collection.AddSingleton<ICompilerService, MipsCompiler>();
        collection.AddSingleton<SettingsService>();
    }
}