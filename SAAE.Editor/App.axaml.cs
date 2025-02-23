using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
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
                var projectSelection = new ProjectSelectionView();
                desktop.MainWindow = projectSelection;
                projectSelection.Show();
                splash.Close();
                await projectSelection.SelectionViewModel.WaitForProjectSelection();

                // finalmente inicializa IDE
                var main = new MainWindow();
                desktop.MainWindow = main;
                main.Show();
                projectSelection.Close();
                
            }

            base.OnFrameworkInitializationCompleted();
        }
        
        private static void OnAppExit(object? sender, ControlledApplicationLifetimeExitEventArgs e) {
            Services.Dispose();
        }
    }
}