using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Editor.Localization;
using Mercury.Editor.Views;

namespace Mercury.Editor.ViewModels;

public partial class AboutViewModel : BaseViewModel<AboutViewModel, AboutView>, IDisposable
{
    [ObservableProperty] private string copyrightString = string.Empty;
    [ObservableProperty] private string versionString = string.Empty;
    [ObservableProperty] private string licenseString = string.Empty;
    [ObservableProperty] private string buildString = string.Empty;
    private readonly Assembly assembly;
    private readonly Version version;
    
    public AboutViewModel()
    {
        LocalizationManager.CultureChanged += OnLocalize;
        assembly = typeof(App).Assembly;
        version = assembly.GetName().Version ?? new Version(0,0);
        OnLocalize(LocalizationManager.CurrentCulture);
        LoadLicense();
    }

    private void OnLocalize(CultureInfo _)
    {
        CopyrightString = AboutResources.CopyrightNoticeValue.Replace("{year}", DateTime.Now.Year.ToString());
        VersionString = string.Format(AboutResources.VersionValue, version);
        BuildString = string.Format(AboutResources.BuildTimeTextValue, BuildInfo.BuildTime.ToLocalTime().ToString("g"));
    }

    private void LoadLicense()
    {
        // if async is used in this method, need to use dispatcher to the ui thread
        string resource = assembly.GetManifestResourceNames().First(x => x.EndsWith("LICENSE.txt", StringComparison.OrdinalIgnoreCase));
        using Stream? stream = assembly.GetManifestResourceStream(resource);
        if (stream == null) return;
        using StreamReader reader = new(stream);
        LicenseString = reader.ReadToEnd();
    }

    public void Dispose()
    {
        LocalizationManager.CultureChanged -= OnLocalize;
    }
}