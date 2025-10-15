using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Localization;
using SAAE.Editor.Services;
using SAAE.Editor.Views;

namespace SAAE.Editor.ViewModels;

public partial class PreferencesViewModel : BaseViewModel<PreferencesViewModel, PreferencesView>, INotifyDataErrorInfo {

    private readonly SettingsService settings = App.Services.GetRequiredService<SettingsService>();
    
    public List<CultureInfo> AvailableLanguages { get; } = [..LocalizationManager.AvailableCultures];
    [ObservableProperty] private int selectedLanguageIndex;

    [ObservableProperty] private string compilerPath = string.Empty;

    [ObservableProperty] private string onlineCheck = string.Empty;
    private string? onlineCheckError = null; 

    [ObservableProperty] private int configVersion;
    
    public void Load() {
        SelectedLanguageIndex = AvailableLanguages.IndexOf(LocalizationManager.CurrentCulture);
        CompilerPath = settings.Preferences.CompilerPath;
        ConfigVersion = settings.Preferences.ConfigVersion;
        OnlineCheck = settings.Preferences.OnlineCheckFrequency.ToString("g");
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
        bool result = TimeSpan.TryParse(value, out TimeSpan ts);
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