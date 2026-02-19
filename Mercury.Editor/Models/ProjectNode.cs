using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Editor.Localization;
using Mercury.Editor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Models;

/// <summary>
/// Os tipos que um nó na arvore de projeto pode ser. 
/// </summary>
[Flags]
public enum ProjectNodeType {
    None = 0,
    Category = 1,
    Folder = 2,
    AssemblyFile = 4,
    UnknownFile = 8,
    Files = AssemblyFile | UnknownFile
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
    private ObservableCollection<NodeContextOption> contextOptions = [];

    [ObservableProperty] private bool isEntryPoint = false;

    public WeakReference<ProjectNode>? ParentReference { get; set; } = null!;
    
    public bool HasContextMenu => ContextOptions.Count > 0;

    [ObservableProperty] 
    private bool isReadOnly;

    [ObservableProperty] private bool isExpanded;

    partial void OnIsExpandedChanged(bool value) {
        var service = App.Services.GetRequiredService<ProjectService>();
        List<OpenProjectNode> nodes = service.GetCurrentProject()!.VisualSettings.OpenProjectNodes;
        OpenProjectNode? node = nodes.Find(x => x.NodeId == Id);
        if (node is null) {
            node = new OpenProjectNode() { NodeId = Id, IsOpen = value };
            nodes.Add(node);
        }
        node.IsOpen = value;
        service.SaveProject();
    }

    public bool IsEffectiveReadOnly => IsReadOnly || (ParentReference?.TryGetTarget(out ProjectNode? parent) == true && parent.IsEffectiveReadOnly);
}

/// <summary>
/// Representa uma opcao de contexto para um nó na árvore de arquivos do projeto.
/// </summary>
public abstract partial class ContextOption<T> : ObservableObject, IDisposable {

    public string Name => Resource.Invoke();

    public required Func<string> Resource { get; init; }

    [ObservableProperty]
    private bool isVisible = true;

    [ObservableProperty]
    private IRelayCommand<T> command = null!;

    public ContextOption() {
        LocalizationManager.CultureChanged += OnLocalize;
    }

    private void OnLocalize(CultureInfo info) {
        OnPropertyChanged(nameof(Name));
    }

    public void Dispose() {
        LocalizationManager.CultureChanged -= OnLocalize;
    }
}

public sealed class NodeContextOption : ContextOption<ProjectNode>;
public sealed class MainContextOption : ContextOption<object>;