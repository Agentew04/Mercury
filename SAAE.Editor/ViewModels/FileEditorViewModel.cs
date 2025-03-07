using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SAAE.Editor.Models;
using SAAE.Editor.Services;

namespace SAAE.Editor.ViewModels;

public partial class FileEditorViewModel : BaseViewModel {

    private readonly FileService fileService = App.Services.GetRequiredService<FileService>();
    
    public FileEditorViewModel() {
        OpenFiles = [
            new OpenFile() {
                Filename = "main.asm",
                Content = "abc",
                IsDirty = true
            },
            new OpenFile() {
                Filename = "geom.asm",
                Content = "def"
            }
        ];
    }
    
    [ObservableProperty]
    private ObservableCollection<OpenFile> openFiles = [];
}