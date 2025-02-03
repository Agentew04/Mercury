using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SAAE.Editor.Views;

namespace SAAE.Editor {
    public partial class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                var splash = new SplashScreen();
                desktop.MainWindow = splash;
                splash.Show();

                try {
                    await splash.ViewModel.Initialize();
                }
                catch (TaskCanceledException) {
                    splash.Close();
                    return;
                }

                var main = new MainWindow();
                desktop.MainWindow = main;
                main.Show();
                splash.Close();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}