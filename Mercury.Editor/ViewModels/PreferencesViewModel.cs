using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Styling;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Editor.Localization;
using Mercury.Editor.Services;
using Mercury.Editor.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mercury.Editor.ViewModels;

public partial class PreferencesViewModel : BaseViewModel<PreferencesViewModel, PreferencesView>, INotifyDataErrorInfo {

    private readonly SettingsService settings = App.Services.GetRequiredService<SettingsService>();
    private readonly ThemeService themeService = App.Services.GetRequiredService<ThemeService>();
    
    public List<CultureInfo> AvailableLanguages { get; } = [..LocalizationManager.AvailableCultures];
    [ObservableProperty] private int selectedLanguageIndex;

    private readonly List<ThemeVariant> themeVariants = [];
    [ObservableProperty] private ObservableCollection<string> themeNames = [];
    [ObservableProperty] private int selectedThemeIndex;

    [ObservableProperty] private string compilerPath = string.Empty;

    [ObservableProperty] private string onlineCheck = string.Empty;
    private string? onlineCheckError; 

    [ObservableProperty] private int configVersion;
    
    
    public void Load() {
        SelectedLanguageIndex = AvailableLanguages.IndexOf(LocalizationManager.CurrentCulture);
        CompilerPath = settings.Preferences.CompilerPath;
        ConfigVersion = settings.Preferences.ConfigVersion;
        OnlineCheck = settings.Preferences.OnlineCheckFrequency.ToString("g");
        
        themeVariants.AddRange(themeService.GetAvailableThemes());
        ThemeNames.AddRange(themeVariants.Select(x => (string)x.Key));
        int themeIdx = ThemeNames.IndexOf(settings.Preferences.Theme);
        if (themeIdx == -1) {
            // default dark mode
            themeIdx = themeVariants.FindIndex(x => ((string)x.Key) == "Dark");
        }
        SelectedThemeIndex = -1;
        SelectedThemeIndex = themeIdx;
    }
    
    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task Apply() {
        
        if (!AvailableLanguages[SelectedLanguageIndex].Equals(LocalizationManager.CurrentCulture)) {
            Logger.LogInformation("Changing to culture: {culture}", AvailableLanguages[SelectedLanguageIndex]);
            LocalizationManager.CurrentCulture = AvailableLanguages[SelectedLanguageIndex];
        }

        settings.Preferences.CompilerPath = CompilerPath;
        if (TimeSpan.TryParse(OnlineCheck, out TimeSpan check)) {
            settings.Preferences.OnlineCheckFrequency = check;
        }
        
        // seta o novo tema
        settings.Preferences.Theme = ThemeNames[SelectedThemeIndex];
        themeService.SetApplicationTheme(themeVariants[SelectedThemeIndex]);
        
        // se limpou, aqui efetiva
        await settings.SaveSettings();
        GetView()?.Close();
    }

    [RelayCommand]
    private void ClearRecentProjects() {
        settings.Preferences.RecentProjects.Clear();
    }

    private bool CanApply() {
        return !HasErrors;
    }

    partial void OnOnlineCheckChanged(string value) {
        bool result = TimeSpan.TryParse(value, out TimeSpan _);
        onlineCheckError = result ? null : PreferencesResources.OnlineCheckErrorValue;
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(OnlineCheck)));
    }

    partial void OnCompilerPathChanged(string value) {
        
    }

    public IEnumerable GetErrors(string? propertyName) {
        if (propertyName == nameof(OnlineCheck)) {
            return new[] { onlineCheckError };
        }

        return null!;
    }

    public bool HasErrors => onlineCheckError != null; 
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
}