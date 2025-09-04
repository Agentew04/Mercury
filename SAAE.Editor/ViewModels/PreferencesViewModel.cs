using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Localization;
using SAAE.Editor.Views;

namespace SAAE.Editor.ViewModels;

public partial class PreferencesViewModel : BaseViewModel<PreferencesViewModel, PreferencesView> {
    
    public List<CultureInfo> AvailableLanguages { get; } = [..LocalizationManager.AvailableCultures];
    [ObservableProperty] private int selectedLanguageIndex;
    
    public void Load() {
        SelectedLanguageIndex = AvailableLanguages.IndexOf(LocalizationManager.CurrentCulture);
    }
    
    [RelayCommand]
    private void Apply() {
        if (!AvailableLanguages[SelectedLanguageIndex].Equals(LocalizationManager.CurrentCulture)) {
            Logger.LogInformation("Changing to culture: {culture}", AvailableLanguages[SelectedLanguageIndex]);
            LocalizationManager.CurrentCulture = AvailableLanguages[SelectedLanguageIndex];
        }
        GetView()?.Close();
    }
}