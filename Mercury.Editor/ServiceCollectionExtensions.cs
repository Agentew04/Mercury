using System.Net.Http;
using System.Net.Http.Headers;
using Mercury.Editor.Services;
using Mercury.Editor.ViewModels;
using Mercury.Editor.ViewModels.Code;
using Mercury.Editor.ViewModels.Execute;
using Mercury.Editor.Views;
using Mercury.Engine.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor;

public static class ServiceCollectionExtensions {

    private static IServiceCollection ConfigureViewModels(this IServiceCollection services) {
        return services
                // singletons
            .AddSingleton<GuideViewModel>()
            .AddSingleton<ProjectSelectionViewModel>()
            .AddSingleton<ProjectViewModel>()
            .AddSingleton<FileEditorViewModel>()
            .AddSingleton<ProblemsViewModel>()
            .AddSingleton<RegisterViewModel>()
            .AddSingleton<OutputViewModel>()
            .AddSingleton<RamViewModel>()
            .AddSingleton<InstructionViewModel>()
            .AddSingleton<LabelViewModel>()
                //transient
            .AddTransient<SplashScreenViewModel>()
            .AddTransient<ProjectConfigurationViewModel>()
            .AddTransient<AboutViewModel>()
            .AddTransient<PreferencesViewModel>();
    }

    private static IServiceCollection ConfigureViews(this IServiceCollection services) {
        return services
            .AddTransient<SplashScreen>()
            .AddTransient<ProjectSelectionView>();

    }

    private static IServiceCollection ConfigureServices(this IServiceCollection services) {
        return services
            .AddKeyedSingleton<ICompilerService, MipsCompiler>(Architecture.Mips)
            .AddSingleton<SettingsService>()
            .AddSingleton<GuideService>()
            .AddSingleton<ProjectService>()
            .AddSingleton<FileService>()
            .AddSingleton<GrammarService>()
            .AddSingleton<ExecuteService>()
            .AddSingleton<UpdaterService>()
            .AddSingleton<ThemeService>()
            .ConfigureHttp();
    }

    private static IServiceCollection ConfigureHttp(this IServiceCollection services) {
        HttpClient httpClient = new(); // reuse the same instance
        HttpRequestHeaders headers = httpClient.DefaultRequestHeaders;
        headers.UserAgent.ParseAdd("MercuryIDE/" + typeof(App).Assembly.GetName().Version);
        services.AddSingleton(httpClient);
        return services;
    }
    
    public static IServiceCollection Configure(this IServiceCollection services) {
        return services
            .ConfigureViewModels()
            .ConfigureViews()
            .ConfigureServices()
            .ConfigureLogging();
    }

    private static IServiceCollection ConfigureLogging(this IServiceCollection collection)
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