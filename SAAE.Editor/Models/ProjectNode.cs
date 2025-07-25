using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SAAE.Editor.Models;

/// <summary>
/// Os tipos que um nó na arvore de projeto pode ser. 
/// </summary>
public enum ProjectNodeType {
    None,
    Category,
    Folder,
    AssemblyFile,
    UnknownFile
}

/// <summary>
/// Representa um nó na arvore de arquivos do projeto atual.
/// </summary>
public partial class ProjectNode : ObservableObject {

    [ObservableProperty]
    private string name = string.Empty;
    
    [ObservableProperty]
    private ProjectNodeType type = ProjectNodeType.None;
    
    [ObservableProperty]
    private ObservableCollection<ProjectNode> children = [];

    public Guid Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasContextMenu))]
    private ObservableCollection<ContextOption> contextOptions = [];

    public WeakReference<ProjectNode> ParentReference { get; set; } = null!;
    
    public bool HasContextMenu => ContextOptions.Count > 0;

    [ObservableProperty] 
    private bool isReadOnly = false;
    
    public bool IsEffectiveReadOnly => IsReadOnly || (ParentReference?.TryGetTarget(out ProjectNode? parent) == true && parent.IsEffectiveReadOnly);
}

/// <summary>
/// Representa uma opcao de contexto para um nó na árvore de arquivos do projeto.
/// </summary>
public partial class ContextOption : ObservableObject {

    [ObservableProperty]
    private string name = "ERRO";

    [ObservableProperty]
    private bool isVisible = true;

    [ObservableProperty]
    private IRelayCommand<ProjectNode> command = null!;

    public ContextOption() {
    }
}

